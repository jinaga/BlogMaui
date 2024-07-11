using Jinaga.Maui.Binding;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteListPage : ContentPage
{
    private readonly SiteListViewModel viewModel;
    private readonly INavigationLifecycleManager navigationLifecycleManager;

    public SiteListPage(SiteListViewModel viewModel, INavigationLifecycleManager navigationLifecycleManager)
    {
        InitializeComponent();
        BindingContext = viewModel;
        this.viewModel = viewModel;
        this.navigationLifecycleManager = navigationLifecycleManager;
    }

    protected override void OnAppearing()
    {
        navigationLifecycleManager.StartManaging(viewModel);
        base.OnAppearing();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        navigationLifecycleManager.StopManaging(viewModel);
        base.OnNavigatedFrom(args);
    }
}