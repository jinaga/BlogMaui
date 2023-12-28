using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga.Maui.Authentication;
using Microsoft.Extensions.Logging;

namespace BlogMaui.Authentication;

public partial class GatekeeperViewModel : ObservableObject
{
    private readonly AuthenticationService authenticationService;
    private readonly AppShellViewModel appShellViewModel;
    private readonly UserProvider userProvider;
    private readonly ILogger<GatekeeperViewModel> logger;

    [ObservableProperty]
    private string error = string.Empty;

    public GatekeeperViewModel(AuthenticationService authenticationService, AppShellViewModel appShellViewModel, UserProvider userProvider, ILogger<GatekeeperViewModel> logger)
    {
        this.authenticationService = authenticationService;
        this.appShellViewModel = appShellViewModel;
        this.userProvider = userProvider;
        this.logger = logger;
    }

    public async void Initialize()
    {
        try
        {
            var user = await authenticationService.Initialize();
            await Task.Delay(1);    // Workaround for shell navigation in OnAppearing: https://github.com/dotnet/maui/issues/6653

            if (user != null)
            {
                userProvider.User = user;
                appShellViewModel.AppState = "LoggedIn";

                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//loggedin");
            }
            else
            {
                appShellViewModel.AppState = "NotLoggedIn";
                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//notloggedin");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing GatekeeperViewModel");
            Error = $"Error while initializing: {ex.GetMessage()}";
            await authenticationService.LogOut();
        }
    }
}
