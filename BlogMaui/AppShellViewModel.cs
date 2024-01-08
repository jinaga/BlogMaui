using BlogMaui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga.Maui.Authentication;
using MetroLog.Maui;
using System.Windows.Input;

namespace BlogMaui;

public partial class AppShellViewModel : ObservableObject
{
    private readonly AuthenticationService authenticationService;
    private readonly UserProvider userProvider;
    private readonly LogController logController = new LogController();

    [ObservableProperty]
    private string appState = "Initializing";
    [ObservableProperty]
    private string error = string.Empty;

    public ICommand LogIn { get; }
    public ICommand LogOut { get; }
    public ICommand ViewLogs { get; }

    public AppShellViewModel(AuthenticationService authenticationService, UserProvider userProvider)
    {
        LogIn = new AsyncRelayCommand(HandleLogIn);
        LogOut = new AsyncRelayCommand(HandleLogOut);
        ViewLogs = logController.GoToLogsPageCommand;

        this.authenticationService = authenticationService;
        this.userProvider = userProvider;

        logController.IsShakeEnabled = true;
    }

    private async Task HandleLogIn()
    {
        try
        {
            Error = string.Empty;
            var user = await authenticationService.Login();
            await Task.Delay(1);

            if (user != null)
            {
                userProvider.User = user;
                AppState = "LoggedIn";

                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//loggedin");
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

            userProvider.User = null;
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
