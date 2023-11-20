namespace BlogMaui.Areas.Account;

public partial class AccountPage : ContentPage
{
    private readonly AccountViewModel viewModel;

    public AccountPage(AccountViewModel viewModel)
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

    protected override void OnDisappearing()
    {
        viewModel.Unload();
        base.OnDisappearing();
    }
}