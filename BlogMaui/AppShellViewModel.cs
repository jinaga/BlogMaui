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
    private readonly AuthenticationService authenticationService;
    private readonly UserProvider userProvider;
    private readonly JinagaClient jinagaClient;
    private readonly LogController logController = new LogController();

    [ObservableProperty]
    private string appState = "Initializing";
    [ObservableProperty]
    private string error = string.Empty;

    [ObservableProperty]
    private string status = string.Empty;

    public ICommand LogIn { get; }
    public ICommand LogOut { get; }
    public ICommand ViewLogs { get; }

    public AppShellViewModel(AuthenticationService authenticationService, UserProvider userProvider, JinagaClient jinagaClient)
    {
        LogIn = new AsyncRelayCommand<string>(HandleLogIn);
        LogOut = new AsyncRelayCommand(HandleLogOut);
        ViewLogs = logController.GoToLogsPageCommand;

        this.authenticationService = authenticationService;
        this.userProvider = userProvider;
        this.jinagaClient = jinagaClient;

        logController.IsShakeEnabled = true;
    }

    public void Load()
    {
        jinagaClient.OnStatusChanged += JinagaClient_OnStatusChanged;
    }

    public void Unload()
    {
        jinagaClient.OnStatusChanged -= JinagaClient_OnStatusChanged;
    }

    private async Task HandleLogIn(string? provider)
    {
        try
        {
            Error = string.Empty;
            var user = await authenticationService.Login(provider ?? "Apple");
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

    private void JinagaClient_OnStatusChanged(JinagaStatus status)
    {
        if ((!status.IsSaving || status.LastSaveError != null) && status.QueueLength > 0)
        {
            // There are facts in the queue, and
            // the client is not saving, or has
            // experienced an error.
            Status = "Red";
            if (status.LastSaveError != null)
            {
                Error = status.LastSaveError.GetMessage();
            }
        }
        else if (status.IsSaving && status.QueueLength == 0)
        {
            // There was an error last time the client
            // tried loading.
            Status = "Yellow";
            if (status.LastLoadError != null)
            {
                Error = status.LastLoadError.GetMessage();
            }
        }
        else
        {
            Status = "Green";
            Error = string.Empty;
        }
    }
}
