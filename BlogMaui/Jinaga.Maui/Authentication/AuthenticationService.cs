using System.Globalization;
using System.Net.Http.Headers;
using Jinaga.Http;
using Jinaga.Maui.Binding;
using Microsoft.Extensions.Logging;

namespace Jinaga.Maui.Authentication;

public class AuthenticationService : IHttpAuthenticationProvider
{
    private readonly ITokenStorage tokenStorage;
    private readonly UserProvider userProvider;
    private readonly IWebAuthenticator webAuthenticator;
    private readonly OAuthClient oauthClient;
    private readonly JinagaClient jinagaClient;
    private readonly Func<JinagaClient, User, UserProfile, Task> updateUserName;
    private readonly ILogger<AuthenticationService> logger;

    private readonly object stateLock = new();
    private bool initialized;
    private AuthenticationResult authenticationState = AuthenticationResult.Empty;

    public AuthenticationService(ITokenStorage tokenStorage, UserProvider userProvider, IWebAuthenticator webAuthenticator, OAuthClient oauthClient, JinagaClient jinagaClient, AuthenticationSettings authenticationSettings, ILogger<AuthenticationService> logger, AuthenticationProviderProxy authenticationProviderProxy)
    {
        this.tokenStorage = tokenStorage;
        this.userProvider = userProvider;
        this.webAuthenticator = webAuthenticator;
        this.oauthClient = oauthClient;
        this.jinagaClient = jinagaClient;
        this.logger = logger;
        updateUserName = authenticationSettings.UpdateUserName;

        authenticationProviderProxy.SetProvider(this);
    }

    public async Task<bool> Initialize()
    {
        lock (stateLock)
        {
            // If called a second time, return true if logged in.
            if (initialized)
            {
                return authenticationState.Token != null;
            }
            initialized = true;
        }

        try
        {
            var loaded = await tokenStorage.LoadTokenAndUser().ConfigureAwait(false);
            if (loaded.Token == null || loaded.User == null)
            {
                // No persisted token, so we are logged out.
                logger.LogInformation("Initialized authentication service with no token");
                return false;
            }

            if (IsExpired(loaded.Token))
            {
                // The token is expired, so refresh it in the background.
                logger.LogInformation("Initialized authentication service with expired token");
                TriggerRefresh(loaded.Token);
            }
            else
            {
                // The token is still valid.
                logger.LogInformation("Initialized authentication service with valid token");
            }

            lock (stateLock)
            {
                authenticationState = loaded;
            }
            userProvider.SetUser(loaded.User);
            return true;
        }
        catch (Exception ex)
        {
            await tokenStorage.SaveTokenAndUser(AuthenticationResult.Empty).ConfigureAwait(false);
            logger.LogError(ex, "Failed to initialize authentication service");
            return false;
        }
    }

    public async Task<bool> Login(string provider)
    {
        lock (stateLock)
        {
            if (authenticationState.Token != null)
            {
                // Already logged in.
                return true;
            }
        }

        try
        {
            AuthenticationResult result = await Authenticate(provider).ConfigureAwait(false);
            if (result.Token == null || result.User == null)
            {
                // Failed to log in.
                logger.LogInformation("Failed to log in");
                return false;
            }

            lock (stateLock)
            {
                authenticationState = result;
            }
            userProvider.SetUser(result.User);
            await tokenStorage.SaveTokenAndUser(result).ConfigureAwait(false);
            logger.LogInformation("Logged in");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log in");
            return false;
        }
    }

    public async Task LogOut()
    {
        lock (stateLock)
        {
            if (authenticationState.Token == null)
            {
                // Already logged out.
                return;
            }
        }

        // TODO: Call the log out endpoint on the server.
        lock (stateLock)
        {
            authenticationState = AuthenticationResult.Empty;
        }
        userProvider.ClearUser();
        await tokenStorage.SaveTokenAndUser(AuthenticationResult.Empty).ConfigureAwait(false);
        logger.LogInformation("Logged out");
    }

    public void SetRequestHeaders(HttpRequestHeaders headers)
    {
        var cachedAuthenticationToken= authenticationState.Token;
        if (cachedAuthenticationToken != null)
        {
            headers.Authorization = new AuthenticationHeaderValue("Bearer", cachedAuthenticationToken.AccessToken);
        }
    }

    public async Task<bool> Reauthenticate()
    {
        var cachedAuthenticationToken = authenticationState.Token;
        if (cachedAuthenticationToken == null)
        {
            // Not logged in.
            return false;
        }

        try
        {
            var refreshedToken = await RefreshToken(cachedAuthenticationToken).ConfigureAwait(false);
            if (refreshedToken == null)
            {
                // Failed to refresh token.
                lock (stateLock)
                {
                    authenticationState = AuthenticationResult.Empty;
                }
                userProvider.ClearUser();
                await tokenStorage.SaveTokenAndUser(AuthenticationResult.Empty).ConfigureAwait(false);
                logger.LogInformation("Failed to refresh token");
                return false;
            }
            else
            {
                // Refreshed token.
                AuthenticationResult authenticationResult;
                lock (stateLock)
                {
                    authenticationResult = new AuthenticationResult(refreshedToken, authenticationState.User);
                    authenticationState = authenticationResult;
                }
                await tokenStorage.SaveTokenAndUser(authenticationResult).ConfigureAwait(false);
                logger.LogInformation("Refreshed token");
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reauthenticate");
            return false;
        }
    }

    private async void TriggerRefresh(AuthenticationToken authenticationToken)
    {
        try
        {
            var refreshedToken = await RefreshToken(authenticationToken).ConfigureAwait(false);
            if (refreshedToken == null)
            {
                // Failed to refresh token.
                lock (stateLock)
                {
                    authenticationState = AuthenticationResult.Empty;
                }
                userProvider.ClearUser();
                await tokenStorage.SaveTokenAndUser(AuthenticationResult.Empty).ConfigureAwait(false);
                logger.LogInformation("Failed to refresh token");
            }
            else
            {
                // Refreshed token.
                AuthenticationResult authenticationResult;
                lock (stateLock)
                {
                    authenticationResult = new AuthenticationResult(refreshedToken, authenticationState.User);
                    authenticationState = authenticationResult;
                }
                await tokenStorage.SaveTokenAndUser(authenticationResult).ConfigureAwait(false);
                logger.LogInformation("Refreshed token");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh token");
        }
    }

    private async Task<AuthenticationResult> Authenticate(string provider)
    {
        string requestUrl = oauthClient.BuildRequestUrl(provider);
        var authResult = await webAuthenticator.AuthenticateAsync(
            new Uri(requestUrl),
            new Uri(oauthClient.CallbackUrl)).ConfigureAwait(false);
        if (authResult == null)
        {
            return AuthenticationResult.Empty;
        }

        if (!authResult.Properties.TryGetValue("state", out string? state) ||
            !authResult.Properties.TryGetValue("code", out string? code))
        {
            logger.LogError("Authentication result did not contain expected properties.");
            return AuthenticationResult.Empty;
        }

        oauthClient.ValidateState(state);
        var tokenResponse = await oauthClient.GetTokenResponse(code).ConfigureAwait(false);
        var authenticationToken = ResponseToToken(tokenResponse);
        var (user, profile) = await jinagaClient.Login().ConfigureAwait(false);
        await updateUserName(jinagaClient, user, profile);
        return new AuthenticationResult(authenticationToken, user);
    }

    private async Task<AuthenticationToken?> RefreshToken(AuthenticationToken authenticationToken)
    {
        var response = await oauthClient.RequestNewToken(authenticationToken.RefreshToken).ConfigureAwait(false);
        if (response == null)
        {
            return null;
        }
        var token = ResponseToToken(response);
        return token;
    }

    private static bool IsExpired(AuthenticationToken token)
    {
        if (DateTime.TryParse(token.ExpiryDate, null, DateTimeStyles.RoundtripKind, out var expiryDate))
        {
            return DateTime.UtcNow > expiryDate.AddMinutes(-5);
        }
        return true;
    }

    private static AuthenticationToken ResponseToToken(TokenResponse tokenResponse)
    {
        return new AuthenticationToken
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiryDate = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                .ToString("O", CultureInfo.InvariantCulture)
        };
    }
}