namespace BlogMaui.Areas.Blog.Posts;

public partial class PostNewPage : ContentPage
{
    public PostNewPage(PostNewViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}