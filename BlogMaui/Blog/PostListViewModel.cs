using BlogMaui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BlogMaui.Blog;
public partial class PostListViewModel : ObservableObject
{
    private readonly JinagaClient jinagaClient;
    private readonly UserProvider userProvider;

    private IObserver? observer;

    [ObservableProperty]
    private bool loading = true;

    [ObservableProperty]
    private string status = "Green";

    public ObservableCollection<PostHeaderViewModel> Posts { get; } = new();

    public ICommand Refresh { get; }

    public PostListViewModel(JinagaClient jinagaClient, UserProvider userProvider)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;

        Refresh = new AsyncRelayCommand(HandleRefresh);
    }

    public async void Load(string domain)
    {
        jinagaClient.OnStatusChanged += JinagaClient_OnStatusChanged;

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
            var postHeaderViewModel = new PostHeaderViewModel(projection.post);
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
            Loading = true;
            await observer.Refresh();
            await jinagaClient.Push();
            Loading = false;
        }
        catch
        {
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
            }
            await observer.Loaded;
            await jinagaClient.Push();
            Loading = false;
        }
        catch
        {
            Loading = false;
        }
    }

    public void Unload()
    {
        jinagaClient.OnStatusChanged -= JinagaClient_OnStatusChanged;

        observer?.Stop();
        Posts.Clear();
    }

    private void JinagaClient_OnStatusChanged(JinagaStatus status)
    {
        if ((!status.IsSaving || status.LastSaveError != null) && status.QueueLength > 0)
        {
            // There are facts in the queue, and
            // the client is not saving, or has
            // experienced an error.
            Status = "Red";
        }
        else if (status.LastLoadError != null)
        {
            // There was an error last time the client
            // tried loading.
            Status = "Yellow";
        }
        else
        {
            Status = "Green";
        }
    }
}
