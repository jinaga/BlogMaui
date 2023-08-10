using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;
using System.Collections.ObjectModel;

namespace MauiApp1.Blog;
internal class PostListViewModel : ObservableObject
{
    private IWatch watch;

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
                    where title.post == post
                    select title.value
                )
            }
        );

        var site = new Site(domain);
        watch = JinagaConfig.j.Watch(postsInBlog, site, projection =>
        {
            var postHeaderViewModel = new PostHeaderViewModel();
            projection.titles.OnAdded(title => MainThread.BeginInvokeOnMainThread(() =>
            {
                postHeaderViewModel.Title = title;
            }));
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Posts.Add(postHeaderViewModel);
            });

            return () => MainThread.BeginInvokeOnMainThread(() =>
            {
                Posts.Remove(postHeaderViewModel);
            });
        });
    }

    public async Task Unload()
    {
        await watch.Stop();
        Posts.Clear();
    }
}
