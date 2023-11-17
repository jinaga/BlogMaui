using Microsoft.Extensions.Logging;

namespace BlogMaui.Account;

public partial class AccountPage : ContentPage
{
	private readonly Logger<AccountPage> logger;

    public AccountPage(Logger<AccountPage> logger)
    {
        this.logger = logger;

        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        logger.LogInformation("OnNavigatedTo AccountPage");
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        logger.LogInformation("OnNavigatingFrom AccountPage");
        base.OnNavigatingFrom(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        logger.LogInformation("OnNavigatedFrom AccountPage");
        base.OnNavigatedFrom(args);
    }

    protected override void OnAppearing()
    {
        logger.LogInformation("OnAppearing AccountPage");
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        logger.LogInformation("OnDisappearing AccountPage");
        base.OnDisappearing();
    }
}