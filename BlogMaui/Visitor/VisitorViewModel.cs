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
            Error = string.Empty;
            bool loggedIn = await authenticationProvider.Login();

            if (loggedIn)
            {
                appShellViewModel.AppState = "LoggedIn";

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