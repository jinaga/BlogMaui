namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteListPage : ContentPage
{
    public SiteListPage()
    {
        BindingContext = new SiteListViewModel();
        InitializeComponent();
    }
}