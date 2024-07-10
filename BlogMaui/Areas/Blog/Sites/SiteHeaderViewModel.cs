using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteHeaderViewModel : ObservableObject
{
    private readonly Site site;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string url = string.Empty;

    public ICommand Select { get; }

    public SiteHeaderViewModel(Site site)
    {
        Select = new AsyncRelayCommand(HandleSelect);
        this.site = site;
    }

    private async Task HandleSelect()
    {
        Dictionary<string, object> parameters = new()
        {
            { "site", site }
        };
        await Shell.Current.GoToAsync("//posts", parameters);
    }
}