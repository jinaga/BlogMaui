using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using System.Windows.Input;

namespace BlogMaui.Authentication;

public partial class GatekeeperViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly UserProvider userProvider;

    [ObservableProperty]
    private string state = "Loading";

    [ObservableProperty]
    private string error = string.Empty;

    public ICommand LogIn { get; }
    public ICommand LogOut { get; }

    public GatekeeperViewModel(OAuth2HttpAuthenticationProvider authenticationProvider, UserProvider userProvider)
    {
        LogIn = new AsyncRelayCommand(HandleLogIn);
        LogOut = new AsyncRelayCommand(HandleLogOut);
        this.authenticationProvider = authenticationProvider;
        this.userProvider = userProvider;
    }

    public async void Initialize()
    {
        try
        {
            bool loggedIn = await authenticationProvider.Initialize();
            await userProvider.Initialize();
            State = loggedIn ? "LoggedIn" : "LoggedOut";
            if (loggedIn)
            {
                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//main");
            }
        }
        catch (Exception ex)
        {
            Error = $"Error while initializing: {GetMessage(ex)}";
            State = "LoggedOut";
        }
    }

    private async Task HandleLogIn()
    {
        try
        {
            bool loggedIn = await authenticationProvider.Login();
            State = loggedIn ? "LoggedIn" : "LoggedOut";
            if (loggedIn)
            {
                // Use two slashes to prevent back navigation to the gatekeeper page.
                await Shell.Current.GoToAsync("//main");
            }
        }
        catch (Exception ex)
        {
            Error = $"Error while logging in: {GetMessage(ex)}";
        }
    }

    public async Task HandleLogOut()
    {
        try
        {
            await authenticationProvider.LogOut();
            await userProvider.ClearUser();
            State = "LoggedOut";
        }
        catch (Exception ex)
        {
            Error = $"Error while logging out: {GetMessage(ex)}";
        }
    }

    private static string GetMessage(Exception? ex)
    {
        var builder = new StringBuilder();
        while (ex != null)
        {
            builder.AppendLine(ex.Message);
            ex = ex.InnerException;
        }
        return builder.ToString();
    }
}
