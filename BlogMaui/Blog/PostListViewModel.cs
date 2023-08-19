using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BlogMaui.Blog;
internal partial class PostListViewModel : ObservableObject
{
    private IObserver observer;

    [ObservableProperty]
    private bool loading = true;
    [ObservableProperty]
    private string message = "Loading...";

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
        observer = JinagaConfig.j.Watch(postsInBlog, site, projection =>
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

        Monitor(observer.Cached, observer.Loaded);
    }

    public ICommand Refresh => new Command(async () =>
    {
        try
        {
            Message = "Checking for updates...";
            Loading = true;
            await observer.Refresh();
            Message = "Posts loaded.";
            Loading = false;
        }
        catch (Exception ex)
        {
            Message = $"Error while loading: {ex.Message}";
            Loading = false;
        }
    });

    private async void Monitor(Task<bool> cached, Task loaded)
    {
        try
        {
            bool wasInCache = await cached;
            if (wasInCache)
            {
                Loading = false;
                Message = "Checking for updates...";
            }
            await loaded;
            Loading = false;
            Message = "Posts loaded.";
        }
        catch (Exception ex)
        {
            Message = $"Error while loading: {ex.Message}";
        }
    }

    public void Unload()
    {
        observer.Stop();
        Posts.Clear();
    }
}
