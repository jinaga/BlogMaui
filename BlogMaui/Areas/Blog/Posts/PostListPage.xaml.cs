using Jinaga.Maui.Binding;

namespace BlogMaui.Areas.Blog.Posts;

// The refresh view does not use the command to indicate
// that the user has initiated a refresh. It executes the
// command whenever the IsRefreshing property becomes true.
// This causes the command to execute twice.
// Until this is fixed, do not use the RefreshView.
// https://github.com/dotnet/maui/issues/6456

public partial class PostListPage : ContentPage
{
    private readonly PostListViewModel viewModel;
    private readonly INavigationLifecycleManager navigationLifecycleManager;

    public PostListPage(PostListViewModel viewModel, INavigationLifecycleManager navigationLifecycleManager)
    {
        InitializeComponent();
        BindingContext = viewModel;
        this.viewModel = viewModel;
        this.navigationLifecycleManager = navigationLifecycleManager;
    }

    protected override void OnAppearing()
    {
        navigationLifecycleManager.OnAppearing(viewModel);
        base.OnAppearing();
    }

    override protected void OnDisappearing()
    {
        navigationLifecycleManager.OnDisappearing(viewModel);
        base.OnDisappearing();
    }
}