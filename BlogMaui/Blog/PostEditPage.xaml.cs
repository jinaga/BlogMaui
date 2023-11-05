namespace BlogMaui.Blog;

public partial class PostEditPage : ContentPage
{
	public PostEditPage(PostEditViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
	}
}