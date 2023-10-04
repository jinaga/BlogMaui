namespace BlogMaui.Authentication;

public partial class GatekeeperPage : ContentPage
{
	private readonly GatekeeperViewModel viewModel;

    public GatekeeperPage(GatekeeperViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        BindingContext = viewModel;
        viewModel.Initialize();
        base.OnAppearing();
    }
}