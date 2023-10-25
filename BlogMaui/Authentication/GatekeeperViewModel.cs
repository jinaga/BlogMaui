﻿using BlogMaui.Jinaga.Maui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogMaui.Authentication;

public partial class GatekeeperViewModel : ObservableObject
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly UserProvider userProvider;
    private readonly AppShellViewModel appShellViewModel;

    [ObservableProperty]
    private string error = string.Empty;

    public GatekeeperViewModel(OAuth2HttpAuthenticationProvider authenticationProvider, UserProvider userProvider, AppShellViewModel appShellViewModel)
    {
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
            appShellViewModel.AppState = loggedIn ? "LoggedIn" : "NotLoggedIn";

            // Use two slashes to prevent back navigation to the gatekeeper page.
            await Shell.Current.GoToAsync(loggedIn
                ? "//loggedin"
                : "//notloggedin");
        }
        catch (Exception ex)
        {
            Error = $"Error while initializing: {ex.GetMessage()}";
        }
    }
}
