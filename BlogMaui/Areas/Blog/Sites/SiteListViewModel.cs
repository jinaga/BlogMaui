using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;

using CommunityToolkit.Mvvm.ComponentModel;

using Jinaga;
using Jinaga.Maui.Authentication;
using Jinaga.Maui.Binding;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteListViewModel : ObservableObject, ILifecycleManaged
{
    private readonly JinagaClient jinagaClient;
    private readonly UserProvider userProvider;
    private readonly ILogger<SiteListViewModel> logger;

    public ICommand NewSiteCommand { get; }

    private UserProvider.Handler? handler;
    private User? user;

    [ObservableProperty]
    private bool loading = false;

    public ObservableCollection<SiteHeaderViewModel> Sites { get; } = new();

    public SiteListViewModel(JinagaClient jinagaClient, UserProvider userProvider, ILogger<SiteListViewModel> logger)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;
        this.logger = logger;

        NewSiteCommand = new AsyncRelayCommand(HandleNewSite, () => user != null);
    }

    public void Load()
    {
        if (handler != null)
        {
            return;
        }

        logger.LogInformation("SiteListViewModel.Load");

        handler = userProvider.AddHandler(user =>
        {
            this.user = user;
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

            var observer = jinagaClient.Watch(sites, user, projection =>
            {
                var siteHeaderViewModel = new SiteHeaderViewModel(projection.site);
                Sites.Add(siteHeaderViewModel);

                projection.names.OnAdded(name =>
                {
                    siteHeaderViewModel.Name = name;
                });
                projection.domains.OnAdded(domain =>
                {
                    siteHeaderViewModel.Url = domain;
                });

                return () =>
                {
                    Sites.Remove(siteHeaderViewModel);
                };
            });

            Monitor(observer);

            return () =>
            {
                this.user = null;
                observer.Stop();
                observer = null;
                Sites.Clear();
            };
        });
    }

    public void Unload()
    {
        logger.LogInformation("SiteListViewModel.Unload");

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

    private async Task HandleNewSite()
    {
        if (user == null)
        {
            return;
        }

        var siteNewViewModel = new SiteNewViewModel(jinagaClient, user);
        var siteNewPage = new SiteNewPage(siteNewViewModel);
        await Shell.Current.Navigation.PushModalAsync(siteNewPage);
    }
}