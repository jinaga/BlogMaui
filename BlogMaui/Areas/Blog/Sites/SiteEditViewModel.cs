using System.Collections.Immutable;
using System.Windows.Input;
using BlogMaui.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteEditViewModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool shouldMergeNames;
    private List<string> nameCandidates;

    [ObservableProperty]
    private string domain = string.Empty;

    [ObservableProperty]
    private bool shouldMergeDomains;
    private List<string> domainCandidates;

    public ICommand MergeNamesCommand { get; }
    public ICommand MergeDomainsCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }
    
    private JinagaClient jinagaClient;
    private Site site;
    private ImmutableList<SiteName> names;
    private ImmutableList<SiteDomain> domains;

    public SiteEditViewModel(JinagaClient jinagaClient, Site site, ImmutableList<SiteName> names, ImmutableList<SiteDomain> domains)
    {
        this.jinagaClient = jinagaClient;
        this.site = site;
        this.names = names;
        this.domains = domains;

        nameCandidates = names.Select(n => n.value).Order().Distinct().ToList();
        Name = nameCandidates.FirstOrDefault() ?? string.Empty;
        ShouldMergeNames = nameCandidates.Count > 1;

        domainCandidates = domains.Select(d => d.value).Order().Distinct().ToList();
        Domain = domainCandidates.FirstOrDefault() ?? string.Empty;
        ShouldMergeDomains = domainCandidates.Count > 1;

        MergeNamesCommand = new AsyncRelayCommand(HandleMergeNames);
        MergeDomainsCommand = new AsyncRelayCommand(HandleMergeDomains);
        SaveCommand = new AsyncRelayCommand(HandleSave);
        CancelCommand = new AsyncRelayCommand(HandleCancel);
        DeleteCommand = new AsyncRelayCommand(HandleDelete);
    }

    private async Task HandleMergeNames()
    {
        var mergeViewModel = new MergeViewModel(nameCandidates, selection => Name = selection);
        var currentPage = Shell.Current.CurrentPage;
        if (currentPage.Parent is NavigationPage navigationPage)
        {
            await navigationPage.PushAsync(new MergePage(mergeViewModel));
        }
    }

    private async Task HandleMergeDomains()
    {
        var mergeViewModel = new MergeViewModel(domainCandidates, selection => Domain = selection);
        var currentPage = Shell.Current.CurrentPage;
        if (currentPage.Parent is NavigationPage navigationPage)
        {
            await navigationPage.PushAsync(new MergePage(mergeViewModel));
        }
    }

    private async Task HandleSave()
    {
        // Record a new site name if it has changed.
        if (names.Count != 1 ||
            names[0].value != Name)
        {
            await jinagaClient.Fact(new SiteName(site, Name, names.ToArray()));
        }

        // Record a new site domain if it has changed.
        if (domains.Count != 1 ||
            domains[0].value != Domain)
        {
            await jinagaClient.Fact(new SiteDomain(site, Domain, domains.ToArray()));
        }

        await Shell.Current.Navigation.PopModalAsync();
    }

    private async Task HandleCancel()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    private async Task HandleDelete()
    {
        await jinagaClient.Fact(new SiteDeleted(site, DateTime.UtcNow));
        await Shell.Current.Navigation.PopModalAsync();
    }
}