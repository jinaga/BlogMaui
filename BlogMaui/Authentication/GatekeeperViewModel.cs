using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga.Maui.Authentication;
using Microsoft.Extensions.Logging;

namespace BlogMaui.Authentication;

public partial class GatekeeperViewModel : ObservableObject
{
    private readonly AuthenticationService authenticationService;
    private readonly AppShellViewModel appShellViewModel;
    private readonly ILogger<GatekeeperViewModel> logger;

    [ObservableProperty]
    private string error = string.Empty;

    public GatekeeperViewModel(AuthenticationService authenticationService, AppShellViewModel appShellViewModel, ILogger<GatekeeperViewModel> logger)
    {
        this.authenticationService = authenticationService;
        this.appShellViewModel = appShellViewModel;
        this.logger = logger;
    }

    public async void Initialize()
    {
        try
        {
            var user = await authenticationService.Initialize();

            if (user != null)
            {
                appShellViewModel.AppState = "LoggedIn";
                Dictionary<string, object> parameters = new()
                {
                    { "user", user }
                };
                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//loggedin", parameters);
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
