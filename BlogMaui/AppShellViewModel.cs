using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using Jinaga.Maui.Authentication;
using Jinaga.Maui.Binding;
using MetroLog.Maui;
using System.Windows.Input;

namespace BlogMaui;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IAuthenticationService authenticationService;
    private readonly JinagaClient jinagaClient;
    private readonly INavigationLifecycleManager navigationLifecycleManager;
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

    public AppShellViewModel(IAuthenticationService authenticationService, JinagaClient jinagaClient, INavigationLifecycleManager navigationLifecycleManager)
    {
        LogIn = new AsyncRelayCommand<string>(HandleLogIn);
        LogOut = new AsyncRelayCommand(HandleLogOut);
        ViewLogs = logController.GoToLogsPageCommand;

        this.authenticationService = authenticationService;
        this.jinagaClient = jinagaClient;
        this.navigationLifecycleManager = navigationLifecycleManager;

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
            var isLoggedIn = await authenticationService.LogIn(provider ?? "Apple");
            await Task.Delay(1);

            if (isLoggedIn)
            {
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
            //navigationLifecycleManager.UnloadInvisible();

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
        else if (status.LastLoadError != null)
        {
            // The client has experienced an error on load.
            Status = "Yellow";
            Error = status.LastLoadError.GetMessage();
        }
        else
        {
            Status = "Green";
            Error = string.Empty;
        }
    }
}
