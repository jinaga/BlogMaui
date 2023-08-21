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

        Monitor();
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

    public ICommand Login => new Command(async () =>
    {
        // Instructions at https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/authentication
        try
        {
            WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri("https://repdev.jinaga.com/N25EVWOs91edOIao79xosTUjEpDHF4HrxOx0GrpZtbMq3ZHqu7DyeiDmEgmhnbBLTdQCBS79OzdzOzTRLi54VQ/auth/apple"),
                new Uri("blogmaui://callback"));

            string accessToken = authResult?.AccessToken;

            // Do something with the token
            Message = $"Received access token {accessToken}";
        }
        catch (Exception ex)
        {
            Message = $"Error while logging in: {ex.Message}";
        }
    });

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
