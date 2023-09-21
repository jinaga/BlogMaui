namespace BlogMaui.Authentication;

public partial class GatekeeperPage : ContentPage
{
	private GatekeeperViewModel viewModel;

	public GatekeeperPage()
	{
		InitializeComponent();
        viewModel = new GatekeeperViewModel();
        BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        viewModel.Initialize();
        base.OnAppearing();
    }
}