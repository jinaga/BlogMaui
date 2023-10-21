using BlogMaui.Exceptions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlogMaui.Authentication;

public partial class GatekeeperViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly UserProvider userProvider;
    private readonly AppShellViewModel appShellViewModel;

    [ObservableProperty]
    private string state = "Loading";

    [ObservableProperty]
    private string error = string.Empty;

    public ICommand LogIn { get; }
    public ICommand LogOut { get; }

    public GatekeeperViewModel(OAuth2HttpAuthenticationProvider authenticationProvider, UserProvider userProvider, AppShellViewModel appShellViewModel)
    {
        LogIn = new AsyncRelayCommand(HandleLogIn);
        LogOut = new AsyncRelayCommand(HandleLogOut);
        this.authenticationProvider = authenticationProvider;
        this.userProvider = userProvider;
        this.appShellViewModel = appShellViewModel;
    }

    public async void Initialize()
    {
        try
        {
            bool loggedIn = await authenticationProvider.Initialize();
            await userProvider.Initialize();
            State = loggedIn ? "LoggedIn" : "LoggedOut";
            appShellViewModel.AppState = loggedIn ? "LoggedIn" : "LoggedOut";
            if (loggedIn)
            {
                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//loggedin/main");
            }
        }
        catch (Exception ex)
        {
            Error = $"Error while initializing: {ex.GetMessage()}";
            State = "LoggedOut";
        }
    }

    private async Task HandleLogIn()
    {
        try
        {
            bool loggedIn = await authenticationProvider.Login();
            State = loggedIn ? "LoggedIn" : "LoggedOut";
            appShellViewModel.AppState = loggedIn ? "LoggedIn" : "LoggedOut";
            if (loggedIn)
            {
                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//loggedin/main");
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
            await authenticationProvider.LogOut();
            await userProvider.ClearUser();
            State = "LoggedOut";
            appShellViewModel.AppState = "LoggedOut";
        }
        catch (Exception ex)
        {
            Error = $"Error while logging out: {ex.GetMessage()}";
        }
    }
}
