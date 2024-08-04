using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteNewViewModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string domain = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    
    private JinagaClient jinagaClient;
    private User creator;

    public SiteNewViewModel(JinagaClient jinagaClient, User creator)
    {
        this.jinagaClient = jinagaClient;
        this.creator = creator;

        SaveCommand = new AsyncRelayCommand(HandleSave);
        CancelCommand = new AsyncRelayCommand(HandleCancel);
    }

    private async Task HandleSave()
    {
        // Record a new Site fact.
        var site = await jinagaClient.Fact(new Site(creator, DateTime.UtcNow));
        await jinagaClient.Fact(new SiteName(site, Name, []));
        await jinagaClient.Fact(new SiteDomain(site, Domain, []));

        await Shell.Current.Navigation.PopModalAsync();
    }

    private async Task HandleCancel()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}