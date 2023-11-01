namespace BlogMaui.Blog;

public partial class PostPage : ContentPage
{
    private readonly PostViewModel viewModel;

    public PostPage(PostViewModel viewModel)
	{
        this.viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnDisappearing()
    {
        viewModel.Unload();
        base.OnDisappearing();
    }

    private void Title_Focused(object sender, FocusEventArgs e)
    {
        viewModel.BeginEditTitle();
    }

    private void Title_Unfocused(object sender, FocusEventArgs e)
    {
        viewModel.EndEditTitle();
    }
}