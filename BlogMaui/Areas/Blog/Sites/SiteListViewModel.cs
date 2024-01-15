using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteListViewModel : ObservableObject
{
    [ObservableProperty]
    private string status = "Fake";

    public ObservableCollection<SiteHeaderViewModel> Sites { get; } = new();

    public SiteListViewModel()
    {
        Sites.Add(new SiteHeaderViewModel
        {
            Name = "Azure DevOps Blog",
            Url = "https://devblogs.microsoft.com/devops/"
        });
        Sites.Add(new SiteHeaderViewModel
        {
            Name = "Azure Blog",
            Url = "https://azure.microsoft.com/en-us/blog/"
        });
        Sites.Add(new SiteHeaderViewModel
        {
            Name = "Visual Studio Blog",
            Url = "https://devblogs.microsoft.com/visualstudio/"
        });
    }
}