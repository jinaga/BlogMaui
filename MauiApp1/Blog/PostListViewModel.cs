using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;
using System.Collections.ObjectModel;

namespace MauiApp1.Blog;
internal class PostListViewModel : ObservableObject
{
    private ObservableCollection<PostHeaderViewModel> postHeaders = new();
    private IWatch watch;

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
            postHeaders.Add(postHeaderViewModel);
            projection.titles.OnAdded(title =>
            {
                postHeaderViewModel.Title = title;
            });

            return () =>
            {
                postHeaders.Remove(postHeaderViewModel);
            };
        });
    }

    public async Task Unload()
    {
        await watch.Stop();
        postHeaders.Clear();
    }
}
