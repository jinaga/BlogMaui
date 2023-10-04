using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BlogMaui.Blog;
public partial class PostListViewModel : ObservableObject
{
    private readonly JinagaClient jinagaClient;

    private IObserver? observer;

    [ObservableProperty]
    private bool loading = true;
    [ObservableProperty]
    private string message = "Loading...";

    public ObservableCollection<PostHeaderViewModel> Posts { get; } = new();

    public ICommand Refresh { get; }

    public PostListViewModel(JinagaClient jinagaClient)
    {
        this.jinagaClient = jinagaClient;

        Refresh = new AsyncRelayCommand(HandleRefresh);
    }

    public async void Load(string domain)
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

        var (user, profile) = await jinagaClient.Login();
        var site = new Site(user, domain);
        var userNames = await jinagaClient.Query(Given<User>.Match((user, facts) =>
            from name in facts.OfType<UserName>()
            where name.user == user &&
                !facts.Any<UserName>(next => next.prior.Contains(name))
            select name
        ), user);
        if (userNames.Count != 1 || userNames.Single().value != profile.DisplayName)
        {
            await jinagaClient.Fact(new UserName(user, profile.DisplayName, userNames.ToArray()));
        }
        observer = jinagaClient.Watch(postsInBlog, site, projection =>
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

        Monitor(observer);
    }

    private async Task HandleRefresh()
    {
        if (observer == null)
        {
            return;
        }

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
    }

    private async void Monitor(IObserver observer)
    {
        try
        {
            bool wasInCache = await observer.Cached;
            if (wasInCache)
            {
                Loading = false;
                Message = "Checking for updates...";
            }
            await observer.Loaded;
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
        observer?.Stop();
        Posts.Clear();
    }
}
