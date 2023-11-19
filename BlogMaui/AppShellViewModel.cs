using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga.Maui.Authentication;
using MetroLog.Maui;
using System.Windows.Input;

namespace BlogMaui;

public partial class AppShellViewModel : ObservableObject
{
    private readonly AuthenticationService authenticationService;
    private readonly LogController logController = new LogController();

    [ObservableProperty]
    private string appState = "Initializing";
    [ObservableProperty]
    private string error = string.Empty;

    public ICommand LogIn { get; }
    public ICommand LogOut { get; }
    public ICommand ViewLogs { get; }

    public AppShellViewModel(AuthenticationService authenticationService)
    {
        LogIn = new AsyncRelayCommand(HandleLogIn);
        LogOut = new AsyncRelayCommand(HandleLogOut);
        ViewLogs = logController.GoToLogsPageCommand;

        this.authenticationService = authenticationService;

        logController.IsShakeEnabled = true;
    }

    private async Task HandleLogIn()
    {
        try
        {
            Error = string.Empty;
            var user = await authenticationService.Login();

            if (user != null)
            {
                AppState = "LoggedIn";

                Dictionary<string, object> parameters = new()
                {
                    { "user", user }
                };
                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//loggedin", parameters);
            }
        }
        catch (Exception ex)
        {
            Error = $"Error while logging in: {ex.GetMessage()}";
        }
    }

    public async Task HandleLogOut()
    {
        try
        {
            Error = string.Empty;
            await authenticationService.LogOut();
            AppState = "NotLoggedIn";

            // Use two slashes to prevent back navigation.
            await Shell.Current.GoToAsync("//notloggedin");
        }
        catch (Exception ex)
        {
            Error = $"Error while logging out: {ex.GetMessage()}";
        }
    }
}
