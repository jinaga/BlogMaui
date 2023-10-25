using BlogMaui.Authentication;
using BlogMaui.Jinaga.Maui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlogMaui;

public partial class AppShellViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly UserProvider userProvider;

    [ObservableProperty]
    private string appState = "Initializing";
    [ObservableProperty]
    private string error = string.Empty;

    public ICommand LogIn { get; }
    public ICommand LogOut { get; }

    public AppShellViewModel(OAuth2HttpAuthenticationProvider authenticationProvider, UserProvider userProvider)
    {
        LogIn = new AsyncRelayCommand(HandleLogIn);
        LogOut = new AsyncRelayCommand(HandleLogOut);

        this.authenticationProvider = authenticationProvider;
        this.userProvider = userProvider;
    }

    private async Task HandleLogIn()
    {
        try
        {
            Error = string.Empty;
            bool loggedIn = await authenticationProvider.Login();

            if (loggedIn)
            {
                AppState = "LoggedIn";

                // Use two slashes to prevent back navigation.
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
