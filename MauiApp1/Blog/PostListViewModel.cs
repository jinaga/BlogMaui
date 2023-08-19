﻿using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;
using System.Collections.ObjectModel;

namespace BlogMaui.Blog;
internal partial class PostListViewModel : ObservableObject
{
    private IWatch watch;

    [ObservableProperty]
    private string message = "Loading";

    public ObservableCollection<PostHeaderViewModel> Posts { get; } = new();

    public void Load(string domain)
    {
        var postsInBlog = Given<Site>.Match((site, facts) =>
            from post in facts.OfType<Post>()
            where post.site == site &&
                !facts.Any<PostDeleted>(deleted => deleted.post == post)
            select new
            {
                post,
                titles = facts.Observable(
                    from title in facts.OfType<PostTitle>()
                    where title.post == post &&
                        !facts.Any<PostTitle>(next => next.prior.Contains(title))
                    select title.value
                )
            }
        );

        var site = new Site(domain);
        watch = JinagaConfig.j.Watch(postsInBlog, site, projection =>
        {
            var postHeaderViewModel = new PostHeaderViewModel();
            projection.titles.OnAdded(title =>
            {
                postHeaderViewModel.Title = title;
            });
            Posts.Add(postHeaderViewModel);

            return () =>
            {
                Posts.Remove(postHeaderViewModel);
            };
        });

        Monitor(watch.Loaded);
    }

    private async void Monitor(Task loaded)
    {
        try
        {
            await loaded;
            Message = "Loading complete";
        }
        catch (Exception ex)
        {
            Message = $"Error while loading: {ex.Message}";
        }
    }

    public void Unload()
    {
        watch.Stop();
        Posts.Clear();
    }
}
