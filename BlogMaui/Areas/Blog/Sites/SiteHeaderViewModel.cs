using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteHeaderViewModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string url = string.Empty;
}