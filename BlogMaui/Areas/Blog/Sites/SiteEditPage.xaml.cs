namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteEditPage : ContentPage
{
	public SiteEditPage(SiteEditViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
	}
}