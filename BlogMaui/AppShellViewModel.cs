using BlogMaui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using Jinaga.Maui.Authentication;
using MetroLog.Maui;
using System.Windows.Input;

namespace BlogMaui;

public partial class AppShellViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly UserProvider userProvider;
    private readonly LogController logController = new LogController();

    [ObservableProperty]
    private string appState = "Initializing";
    [ObservableProperty]
    private string error = string.Empty;

    public ICommand LogIn { get; }
    public ICommand LogOut { get; }
    public ICommand ViewLogs { get; }

    public AppShellViewModel(OAuth2HttpAuthenticationProvider authenticationProvider, UserProvider userProvider)
    {
        LogIn = new AsyncRelayCommand(HandleLogIn);
        LogOut = new AsyncRelayCommand(HandleLogOut);
        ViewLogs = logController.GoToLogsPageCommand;

        this.authenticationProvider = authenticationProvider;
        this.userProvider = userProvider;

        logController.IsShakeEnabled = true;
    }

    private async Task HandleLogIn()
    {
        try
        {
            Error = string.Empty;
            bool loggedIn = await authenticationProvider.Login();
            var user = loggedIn ? await userProvider.GetUser() : null;

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
            await authenticationProvider.LogOut();
            await userProvider.ClearUser();
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
