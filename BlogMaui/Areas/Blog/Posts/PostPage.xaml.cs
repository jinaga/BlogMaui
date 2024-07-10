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

    protected override void OnAppearing()
    {
        viewModel.Load();
        base.OnAppearing();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        if (Shell.Current.Navigation.NavigationStack.Any(page => page == this) ||
            Shell.Current.Navigation.ModalStack.Any(page => page == this))
        {
            return;
        }

        viewModel.Unload();
        base.OnNavigatedFrom(args);
    }
}