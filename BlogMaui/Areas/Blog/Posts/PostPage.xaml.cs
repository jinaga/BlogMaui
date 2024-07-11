namespace BlogMaui.Areas.Blog.Posts;

public partial class PostPage : ContentPage
{
    private readonly PostViewModel viewModel;

    public PostPage(PostViewModel viewModel)
	{
        this.viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        viewModel.Load();
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        viewModel.Unload();
        base.OnNavigatedFrom(args);
    }
}