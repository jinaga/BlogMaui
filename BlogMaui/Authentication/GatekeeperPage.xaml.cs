using Jinaga.Maui.Binding;

namespace BlogMaui.Authentication;

public partial class GatekeeperPage : ContentPage
{
	private readonly GatekeeperViewModel viewModel;
    private readonly INavigationLifecycleManager navigationLifecycleManager;

    public GatekeeperPage(GatekeeperViewModel viewModel, INavigationLifecycleManager navigationLifecycleManager)
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

    protected override void OnDisappearing()
    {
        navigationLifecycleManager.OnDisappearing(viewModel);
        base.OnDisappearing();
    }
}