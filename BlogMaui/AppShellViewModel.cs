using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlogMaui;

public partial class AppShellViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;

    [ObservableProperty]
    private string appState = "Initializing";
    [ObservableProperty]
    private string error = string.Empty;

    public ICommand LogIn { get; }

    public AppShellViewModel(OAuth2HttpAuthenticationProvider authenticationProvider)
    {
        this.authenticationProvider = authenticationProvider;

        LogIn = new AsyncRelayCommand(HandleLogIn);
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
}
