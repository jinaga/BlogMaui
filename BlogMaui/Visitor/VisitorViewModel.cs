using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlogMaui.Visitor;

public partial class VisitorViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly AppShellViewModel appShellViewModel;

    [ObservableProperty]
    private string error = string.Empty;

    public VisitorViewModel(OAuth2HttpAuthenticationProvider authenticationProvider, AppShellViewModel appShellViewModel)
    {
        this.authenticationProvider = authenticationProvider;
        this.appShellViewModel = appShellViewModel;

        LogIn = new AsyncRelayCommand(HandleLogIn);
    }

    public ICommand LogIn { get; }

    private async Task HandleLogIn()
    {
        try
        {
            bool loggedIn = await authenticationProvider.Login();
            appShellViewModel.AppState = loggedIn ? "LoggedIn" : "NotLoggedIn";

            // Use two slashes to prevent back navigation to the gatekeeper page.
            await Shell.Current.GoToAsync(loggedIn
                ? "//loggedin"
                : "//notloggedin");
        }
        catch (Exception ex)
        {
            Error = $"Error while logging in: {ex.GetMessage()}";
        }
    }
}