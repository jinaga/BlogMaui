using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga.Maui.Authentication;
using Jinaga.Maui.Binding;
using Microsoft.Extensions.Logging;

namespace BlogMaui.Authentication;

public partial class GatekeeperViewModel : ObservableObject, ILifecycleManaged
{
    private readonly IAuthenticationService authenticationService;
    private readonly AppShellViewModel appShellViewModel;
    private readonly ILogger<GatekeeperViewModel> logger;

    [ObservableProperty]
    private string error = string.Empty;

    public GatekeeperViewModel(IAuthenticationService authenticationService, AppShellViewModel appShellViewModel, ILogger<GatekeeperViewModel> logger)
    {
        this.authenticationService = authenticationService;
        this.appShellViewModel = appShellViewModel;
        this.logger = logger;
    }

    public async void Load()
    {
        try
        {
            var loggedIn = await authenticationService.Initialize();
            await Task.Delay(1);    // Workaround for shell navigation in OnAppearing: https://github.com/dotnet/maui/issues/6653

            if (loggedIn)
            {
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
        }
    }

    public void Unload()
    {
        logger.LogInformation("Unloading gatekeeper");
    }
}
