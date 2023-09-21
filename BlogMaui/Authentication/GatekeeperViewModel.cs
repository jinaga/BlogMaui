using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using System.Windows.Input;

namespace BlogMaui.Authentication;

internal partial class GatekeeperViewModel : ObservableObject
{
    [ObservableProperty]
    private bool loading = true;

    [ObservableProperty]
    private bool loggedIn = false;

    [ObservableProperty]
    private string error = string.Empty;

    public ICommand LogIn { get; }

    public GatekeeperViewModel()
    {
        LogIn = new AsyncRelayCommand(HandleLogIn);
    }

    public async void Initialize()
    {
        try
        {
            var loggedIn = await JinagaConfig.AuthenticationProvider.Initialize();
            Loading = false;
            LoggedIn = loggedIn;
        }
        catch (Exception ex)
        {
            Error = $"Error while initializing: {GetMessage(ex)}";
            Loading = false;
        }
    }

    private async Task HandleLogIn()
    {
        try
        {
            var loggedIn = await JinagaConfig.AuthenticationProvider.Login();
            LoggedIn = loggedIn;
        }
        catch (Exception ex)
        {
            Error = $"Error while logging in: {GetMessage(ex)}";
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
