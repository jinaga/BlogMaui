namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteNewPage : ContentPage
{
    public SiteNewPage(SiteNewViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}