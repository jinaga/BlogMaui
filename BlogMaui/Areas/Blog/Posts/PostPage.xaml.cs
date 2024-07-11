using Jinaga.Maui.Binding;

namespace BlogMaui.Areas.Blog.Posts;

public partial class PostPage : ContentPage
{
    private readonly PostViewModel viewModel;
    private readonly INavigationLifecycleManager navigationLifecycleManager;

    public PostPage(PostViewModel viewModel, INavigationLifecycleManager navigationLifecycleManager)
    {
        this.viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
        this.navigationLifecycleManager = navigationLifecycleManager;
    }

    protected override void OnAppearing()
    {
        navigationLifecycleManager.OnAppearing(viewModel);
        base.OnAppearing();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        navigationLifecycleManager.OnDisappearing(viewModel);
        base.OnNavigatedFrom(args);
    }
}