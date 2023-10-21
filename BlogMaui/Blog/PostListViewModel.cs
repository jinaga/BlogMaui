using BlogMaui.Authentication;
using BlogMaui.Exceptions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BlogMaui.Blog;
public partial class PostListViewModel : ObservableObject
{
    private readonly JinagaClient jinagaClient;
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly UserProvider userProvider;
    private readonly AppShellViewModel appShellViewModel;

    private IObserver? observer;

    [ObservableProperty]
    private bool loading = true;
    [ObservableProperty]
    private string message = "Loading...";

    public ObservableCollection<PostHeaderViewModel> Posts { get; } = new();

    public ICommand Refresh { get; }
    public ICommand LogOut { get; }

    public PostListViewModel(JinagaClient jinagaClient, UserProvider userProvider, OAuth2HttpAuthenticationProvider authenticationProvider, AppShellViewModel appShellViewModel)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;
        this.authenticationProvider = authenticationProvider;
        this.appShellViewModel = appShellViewModel;

        Refresh = new AsyncRelayCommand(HandleRefresh);
        LogOut = new AsyncRelayCommand(HandleLogOut);
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

        var user = await userProvider.GetUser();
        if (user == null)
        {
            return;
        }

        var site = new Site(user, domain);
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

    public async Task HandleLogOut()
    {
        try
        {
            Message = "Logging out...";
            await authenticationProvider.LogOut();
            await userProvider.ClearUser();
            appShellViewModel.AppState = "LoggedOut";
            Message = "Logged out.";
            await Shell.Current.GoToAsync("//notloggedin/visitor");
        }
        catch (Exception ex)
        {
            Message = $"Error while logging out: {ex.GetMessage()}";
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
