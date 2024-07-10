using Jinaga.Maui;

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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        viewModel.Load();
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        if (this.PageIsInStack())
        {
            return;
        }

        viewModel.Unload();
        base.OnNavigatedFrom(args);
    }
}