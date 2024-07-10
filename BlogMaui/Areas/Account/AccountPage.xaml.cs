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