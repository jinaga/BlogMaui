using System.Collections.Immutable;
using System.Collections.ObjectModel;
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

    private List<string> nameCandidates;

    [ObservableProperty]
    private string domain = string.Empty;

    public ObservableCollection<string> DomainCandidates { get; }
    [ObservableProperty]
    private string selectedDomainCandidate = string.Empty;

    public ICommand MergeCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    
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

        Name = names
            .Select(n => n.value)
            .Order()
            .FirstOrDefault() ?? string.Empty;
        nameCandidates = names.Select(n => n.value).ToList();

        Domain = domains
            .Select(d => d.value)
            .Order()
            .FirstOrDefault() ?? string.Empty;
        DomainCandidates = new ObservableCollection<string>(domains.Select(d => d.value));

        MergeCommand = new AsyncRelayCommand(HandleMerge);
        SaveCommand = new AsyncRelayCommand(HandleSave);
        CancelCommand = new AsyncRelayCommand(HandleCancel);
    }

    partial void OnSelectedDomainCandidateChanged(string value)
    {
        Domain = value;
    }

    private async Task HandleMerge()
    {
        var mergeViewModel = new MergeViewModel(nameCandidates, selection => Name = selection);
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
}