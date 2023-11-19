using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;
using Jinaga.Maui.Authentication;
using Microsoft.Extensions.Logging;

namespace BlogMaui.Authentication;

public partial class GatekeeperViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly AppShellViewModel appShellViewModel;
    private readonly JinagaClient jinagaClient;
    private readonly ILogger<GatekeeperViewModel> logger;

    [ObservableProperty]
    private string error = string.Empty;

    public GatekeeperViewModel(OAuth2HttpAuthenticationProvider authenticationProvider, AppShellViewModel appShellViewModel, JinagaClient jinagaClient, ILogger<GatekeeperViewModel> logger)
    {
        this.authenticationProvider = authenticationProvider;
        this.appShellViewModel = appShellViewModel;
        this.jinagaClient = jinagaClient;
        this.logger = logger;
    }

    public async void Initialize()
    {
        try
        {
            bool loggedIn = await authenticationProvider.Initialize();
            var user = await authenticationProvider.GetUser(jinagaClient, loggedIn);

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
            await authenticationProvider.LogOut();
        }
    }
}
