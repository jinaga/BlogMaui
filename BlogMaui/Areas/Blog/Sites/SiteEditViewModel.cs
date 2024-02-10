using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteEditViewModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    public ObservableCollection<string> NameCandidates { get; }
    [ObservableProperty]
    private string selectedNameCandidate = string.Empty;

    [ObservableProperty]
    private string domain = string.Empty;

    public ObservableCollection<string> DomainCandidates { get; }
    [ObservableProperty]
    private string selectedDomainCandidate = string.Empty;

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
        NameCandidates = new ObservableCollection<string>(names.Select(n => n.value));

        Domain = domains
            .Select(d => d.value)
            .Order()
            .FirstOrDefault() ?? string.Empty;
        DomainCandidates = new ObservableCollection<string>(domains.Select(d => d.value));

        SaveCommand = new AsyncRelayCommand(HandleSave);
        CancelCommand = new AsyncRelayCommand(HandleCancel);
    }

    partial void OnSelectedNameCandidateChanged(string value)
    {
        Name = value;
    }

    partial void OnSelectedDomainCandidateChanged(string value)
    {
        Domain = value;
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