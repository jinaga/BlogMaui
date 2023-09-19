using BlogMaui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public ICommand Refresh { get; }
    public ICommand Login { get; }

    public PostListViewModel()
    {
        Refresh = new AsyncRelayCommand(HandleRefresh);
        Login = new AsyncRelayCommand(HandleLogin);
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

        var (user, profile) = await JinagaConfig.j.Login();
        var site = new Site(user, domain);
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

        Monitor();
    }

    private async Task HandleRefresh()
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
    }

    private async Task HandleLogin()
    {
        // Instructions at https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/authentication
        try
        {
            var settings = new Settings();
            var client = new OAuthClient(
                settings.AuthUrl,
                settings.AccessTokenUrl,
                settings.CallbackUrl,
                settings.ClientId,
                settings.Scope
            );
            string requestUrl = client.BuildRequestUrl();
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(requestUrl),
                new Uri(settings.CallbackUrl));

            string state = authResult.Properties["state"];
            string code = authResult.Properties["code"];

            client.ValidateState(state);
            var tokenResponse = await client.GetTokenResponse(code);

            // Do something with the token

            Message = $"Received access token {tokenResponse.AccessToken}";
        }
        catch (Exception ex)
        {
            Message = $"Error while logging in: {ex.Message}";
        }
    }

    private async void Monitor()
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
        observer.Stop();
        Posts.Clear();
    }
}
