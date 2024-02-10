using System.Collections.ObjectModel;
using BlogMaui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteListViewModel : ObservableObject
{
    private readonly JinagaClient jinagaClient;
    private readonly UserProvider userProvider;

    private IObserver? observer;

    [ObservableProperty]
    private bool loading = false;

    public ObservableCollection<SiteHeaderViewModel> Sites { get; } = new();

    public SiteListViewModel(JinagaClient jinagaClient, UserProvider userProvider)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;
    }

    public void Load()
    {
        if (userProvider.User == null || observer != null)
        {
            return;
        }

        Loading = true;

        var sites = Given<User>.Match((user, facts) =>
            from site in facts.OfType<Site>()
            where site.creator == user
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

        observer = jinagaClient.Watch(sites, userProvider.User, projection =>
        {
            Sites.Clear();
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
    }

    public void Unload()
    {
        observer?.Stop();
        observer = null;
        Sites.Clear();
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
        finally
        {
            Loading = false;
        }
    }
}