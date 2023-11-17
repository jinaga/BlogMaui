using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga.Maui.Authentication;
using Microsoft.Extensions.Logging;

namespace BlogMaui.Authentication;

public partial class GatekeeperViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly UserProvider userProvider;
    private readonly AppShellViewModel appShellViewModel;
    private readonly ILogger<GatekeeperViewModel> logger;

    [ObservableProperty]
    private string error = string.Empty;

    public GatekeeperViewModel(OAuth2HttpAuthenticationProvider authenticationProvider, UserProvider userProvider, AppShellViewModel appShellViewModel, ILogger<GatekeeperViewModel> logger)
    {
        this.authenticationProvider = authenticationProvider;
        this.userProvider = userProvider;
        this.appShellViewModel = appShellViewModel;
        this.logger = logger;
    }

    public async void Initialize()
    {
        try
        {
            bool loggedIn = await authenticationProvider.Initialize();
            await userProvider.Initialize();
            var user = await userProvider.GetUser();
            appShellViewModel.AppState = loggedIn ? "LoggedIn" : "NotLoggedIn";

            // Use two slashes to prevent back navigation to the gatekeeper page.
            if (loggedIn && user != null)
            {
                Dictionary<string, object> parameters = new()
                {
                    { "user", user }
                };
                await Shell.Current.GoToAsync("//loggedin", parameters);
            }
            else
            {
                await Shell.Current.GoToAsync("//notloggedin");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing GatekeeperViewModel");
            Error = $"Error while initializing: {ex.GetMessage()}";
            await authenticationProvider.LogOut();
            await userProvider.ClearUser();
        }
    }
}
