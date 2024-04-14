using System.Collections.ObjectModel;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using BlogMaui.Areas.Blog.Sites;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Jinaga;

namespace BlogMaui.Areas.Blog.Posts;
public partial class PostListViewModel : ObservableObject, IQueryAttributable
{
    private readonly JinagaClient jinagaClient;
    private readonly ILogger<PostListViewModel> logger;

    private Site? site;
    private IObserver? observer;
    private IObserver? nameObserver;

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private bool loading = false;

    public ObservableCollection<PostHeaderViewModel> Posts { get; } = new();

    public ICommand Edit { get; }
    public ICommand Refresh { get; }

    public PostListViewModel(JinagaClient jinagaClient, ILogger<PostListViewModel> logger)
    {
        this.jinagaClient = jinagaClient;
        this.logger = logger;

        Edit = new AsyncRelayCommand(HandleEdit);
        Refresh = new AsyncRelayCommand(HandleRefresh);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            site = query.GetParameter<Site>("site");
            Loading = true;

            var namesOfSite = Given<Site>.Match((site, facts) =>
                from name in facts.OfType<SiteName>()
                where name.site == site &&
                    !facts.Any<SiteName>(next => next.prior.Contains(name))
                select name.value
            );

            nameObserver = jinagaClient.Watch(namesOfSite, site, projection =>
            {
                Title = projection;
            });

            var postsInBlog = Given<Site>.Match((site, facts) =>
                from post in facts.OfType<Post>()
                where post.site == site &&
                    !facts.Any<PostDeleted>(deleted => deleted.post == post &&
                        !facts.Any<PostRestored>(restored => restored.deleted == deleted))
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
        catch (Exception x)
        {
            logger.LogError(x, "Error while applying query attributes");
        }
    }

    public void Unload()
    {
        try
        {
            observer?.Stop();
            observer = null;
            nameObserver?.Stop();
            nameObserver = null;
            Posts.Clear();
        }
        catch (Exception x)
        {
            logger.LogError(x, "Error while unloading");
        }
    }

    private async Task HandleEdit()
    {
        try
        {
            if (site != null)
            {
                var namesOfSite = Given<Site>.Match((site, facts) =>
                    from name in facts.OfType<SiteName>()
                    where name.site == site &&
                        !facts.Any<SiteName>(next => next.prior.Contains(name))
                    select name
                );
                var names = await jinagaClient.Local.Query(namesOfSite, site);

                var domainsOfSite = Given<Site>.Match((site, facts) =>
                    from domain in facts.OfType<SiteDomain>()
                    where domain.site == site &&
                        !facts.Any<SiteDomain>(next => next.prior.Contains(domain))
                    select domain
                );
                var domains = await jinagaClient.Local.Query(domainsOfSite, site);

                var viewModel = new SiteEditViewModel(jinagaClient, site, names, domains);
                var page = new NavigationPage(new SiteEditPage(viewModel));
                await Shell.Current.Navigation.PushModalAsync(page);
            }
        }
        catch (Exception x)
        {
            logger.LogError(x, "Error while handling edit");
        }
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
            await Task.WhenAll(
                observer.Refresh(),
                jinagaClient.Push());
        }
        catch (Exception x)
        {
            logger.LogError(x, "Error while refreshing");
        }
        finally
        {
            Loading = false;
        }
    }

    private async void Monitor(IObserver observer)
    {
        try
        {
            bool wasInCache = await observer.Cached;
            if (!wasInCache)
            {
                await Task.WhenAll(
                    observer.Loaded,
                    jinagaClient.Push());
            }
        }
        catch (Exception x)
        {
            logger.LogError(x, "Error while loading");
        }
        finally
        {
            Loading = false;
        }
    }
}