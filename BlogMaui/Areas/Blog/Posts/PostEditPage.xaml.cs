namespace BlogMaui.Areas.Blog.Posts;

public partial class PostEditPage : ContentPage
{
	public PostEditPage(PostEditViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
	}
}