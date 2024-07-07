using Microsoft.Extensions.Logging;

using CommunityToolkit.Mvvm.ComponentModel;

using Jinaga;
using Jinaga.Maui.Binding;
using BlogMaui.Binding;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteListViewModel : ObservableObject
{
    private readonly JinagaClient jinagaClient;
    private readonly UserProvider userProvider;
    private readonly ILogger<SiteListViewModel> logger;

    private UserProvider.Handler? handler;

    [ObservableProperty]
    private bool loading = false;

    public ViewModelCollection Sites { get; } = new();

    public SiteListViewModel(JinagaClient jinagaClient, UserProvider userProvider, ILogger<SiteListViewModel> logger)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;
        this.logger = logger;
    }

    public void Load()
    {
        if (handler != null)
        {
            return;
        }

        handler = userProvider.AddHandler(user =>
        {
            Loading = true;

            var sites = Given<User>.Match((user, facts) =>
                from site in facts.OfType<Site>()
                where site.creator == user &&
                    !facts.Any<SiteDeleted>(deleted => deleted.site == site &&
                        !facts.Any<SiteRestored>(restored => restored.deleted == deleted)
                    )
                select new
                {
                    site,
                    names = facts.Observable(
                        from name in facts.OfType<SiteName>()
                        where name.site == site &&
                            !facts.Any<SiteName>(next => next.prior.Contains(name))
                        select name.value
                    ),
                    domains = facts.Observable(
                        from domain in facts.OfType<SiteDomain>()
                        where domain.site == site &&
                            !facts.Any<SiteDomain>(next => next.prior.Contains(domain))
                        select domain.value
                    )
                }
            );

            var observer = Sites.Watch(jinagaClient, sites, user, projection =>
                new SiteListViewModel(jinagaClient, userProvider, logger),
                (projection, viewModel) =>
                {
                    viewModel.Name = projection.names.LastOrDefault() ?? "Untitled";
                    viewModel.Url = projection.domains.LastOrDefault() ?? "example.com";
                });

            Monitor(observer);

            return () =>
            {
                observer.Stop();
            };
        });
    }

    public void Unload()
    {
        if (handler != null)
        {
            userProvider.RemoveHandler(handler);
            handler = null;
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
                    jinagaClient.Push()
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error monitoring SiteListViewModel");
        }
        finally
        {
            Loading = false;
        }
    }
}