using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Jinaga.Http;
using Jinaga.Maui.Binding;
using Microsoft.Extensions.Logging;

namespace Jinaga.Maui.Authentication;

public class AuthenticationService : IHttpAuthenticationProvider
{
    private const string PublicKeyKey = "Jinaga.PublicKey";
    private const string AuthenticationTokenKey = "Jinaga.AuthenticationToken";

    private readonly UserProvider userProvider;
    private readonly IWebAuthenticator webAuthenticator;
    private readonly OAuthClient oauthClient;
    private readonly JinagaClient jinagaClient;
    private readonly Func<JinagaClient, User, UserProfile, Task> updateUserName;
    private readonly ILogger<AuthenticationService> logger;

    enum State
    {
        Uninitialized,
        LoggedOut,
        LoggingIn,
        LoggedIn,
        Refreshing,
        Offline
    }

    private volatile State state = State.Uninitialized;
    private AuthenticationToken? authenticationToken;
    private User? user;
    private Task<bool> refreshTask = Task.FromResult(false);

    public AuthenticationService(UserProvider userProvider, IWebAuthenticator webAuthenticator, OAuthClient oauthClient, JinagaClient jinagaClient, AuthenticationSettings authenticationSettings, ILogger<AuthenticationService> logger)
    {
        this.userProvider = userProvider;
        this.webAuthenticator = webAuthenticator;
        this.oauthClient = oauthClient;
        this.jinagaClient = jinagaClient;
        this.logger = logger;
        updateUserName = authenticationSettings.UpdateUserName;
    }

    public async Task<bool> Initialize()
    {
        // If called a second time, return true if logged in, false if logged out.
        if (state != State.Uninitialized)
        {
            return state != State.LoggedOut;
        }

        try
        {
            // Load the state from secure storage.
            logger.LogInformation("Loading authentication token");
            string? tokenJson = await SecureStorage.GetAsync(AuthenticationTokenKey).ConfigureAwait(false);
            string? publicKey = await SecureStorage.GetAsync(PublicKeyKey).ConfigureAwait(false);
            if (tokenJson != null && publicKey != null)
            {
                // Deserialize the token.
                AuthenticationToken? loadedAuthenticationToken = JsonSerializer.Deserialize<AuthenticationToken>(tokenJson);
                authenticationToken = loadedAuthenticationToken;
                user = new User(publicKey);
                userProvider.SetUser(user);
                if (IsTokenExpired())
                {
                    // If the token is expired, start refreshing it.
                    logger.LogInformation("Token expired, refreshing");
                    state = State.Refreshing;
                    BeginRefresh();
                }
                else
                {
                    logger.LogInformation("Token loaded");
                    state = State.LoggedIn;
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            // If there was an error loading the state, log it and clear the state.
            logger.LogError(ex, "Failed to load authentication token");
            authenticationToken = null;
            user = null;
            userProvider.ClearUser();
            await SaveState().ConfigureAwait(false);
        }
        return false;
    }

    public async Task<bool> Login(string provider)
    {
        if (state == State.Offline)
        {
            var refreshed = await RefreshToken().ConfigureAwait(false);
            if (refreshed)
            {
                state = State.LoggedIn;
                return true;
            }
            else
            {
                state = State.LoggedOut;
            }
        }

        if (state == State.Refreshing)
        {
            return await refreshTask.ConfigureAwait(false);
        }

        if (state != State.LoggedOut)
        {
            return state == State.LoggedIn;
        }

        state = State.LoggingIn;
        var loggedIn = await Authenticate(provider).ConfigureAwait(false);
        if (loggedIn)
        {
            state = State.LoggedIn;
            return true;
        }
        else
        {
            state = State.LoggedOut;
            return false;
        }
    }

    public async Task LogOut()
    {
        state = State.LoggedOut;
        authenticationToken = null;
        user = null;
        userProvider.ClearUser();
        await SaveState().ConfigureAwait(false);
    }

    public void SetRequestHeaders(HttpRequestHeaders headers)
    {
        var cachedAuthenticationToken = authenticationToken;
        if (cachedAuthenticationToken != null)
        {
            headers.Authorization = new AuthenticationHeaderValue("Bearer", cachedAuthenticationToken.AccessToken);
        }
    }

    public Task<bool> Reauthenticate()
    {
        if (state == State.Offline)
        {
            return Task.FromResult(false);
        }

        if (state == State.Refreshing)
        {
            return refreshTask;
        }

        if (state == State.LoggedIn)
        {
            return Task.FromResult(true);
        }

        state = State.Refreshing;
        refreshTask = RefreshToken();
        return refreshTask;
    }

    private void BeginRefresh()
    {
        refreshTask = RefreshToken();
        refreshTask.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                logger.LogError(t.Exception, "Failed to refresh token");
            }
            else if (t.Result)
            {
                state = State.LoggedIn;
            }
            else
            {
                state = State.LoggedOut;
            }
        });
    }

    private async Task<bool> Authenticate(string provider)
    {
        logger.LogInformation("Logging in with {Provider}", provider);

        string requestUrl = oauthClient.BuildRequestUrl(provider);
        var authResult = await webAuthenticator.AuthenticateAsync(
            new Uri(requestUrl),
            new Uri(oauthClient.CallbackUrl)).ConfigureAwait(false);
        if (authResult == null)
        {
            logger.LogInformation("Authentication cancelled");
            return false;
        }

        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation("Received authentication result");

        try
        {
            if (!authResult.Properties.TryGetValue("state", out string? state) ||
                !authResult.Properties.TryGetValue("code", out string? code))
            {
                logger.LogError("Authentication result did not contain expected properties.");
                return false;
            }

            oauthClient.ValidateState(state);
            var tokenResponse = await oauthClient.GetTokenResponse(code).ConfigureAwait(false);
            await UpdateAuthenticationState(tokenResponse).ConfigureAwait(false);
            logger.LogInformation("Token received in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get token");
            return false;
        }
    }

    private async Task<bool> RefreshToken()
    {
        if (authenticationToken?.RefreshToken == null)
        {
            return false;
        }
    
        try
        {
            var tokenResponse = await RequestNewTokenSafe(authenticationToken.RefreshToken).ConfigureAwait(false);
            if (tokenResponse == null)
            {
                await ClearAuthenticationState().ConfigureAwait(false);
                return false;
            }
    
            await UpdateAuthenticationState(tokenResponse).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh token");
            await ClearAuthenticationState().ConfigureAwait(false);
            return false;
        }
    }
    
    private async Task<TokenResponse?> RequestNewTokenSafe(string refreshToken)
    {
        return await oauthClient.RequestNewToken(refreshToken).ConfigureAwait(false);
    }
    
    private async Task ClearAuthenticationState()
    {
        authenticationToken = null;
        this.user = null;
        userProvider.ClearUser();
        await SaveState().ConfigureAwait(false);
    }
    
    private async Task UpdateAuthenticationState(TokenResponse tokenResponse)
    {
        authenticationToken = ResponseToToken(tokenResponse);
        var (user, profile) = await jinagaClient.Login().ConfigureAwait(false);
        await updateUserName(jinagaClient, user, profile);
        this.user = user;
        await SaveState().ConfigureAwait(false);
    }

    private bool IsTokenExpired()
    {
        // Check for token expiration
        if (authenticationToken == null)
        {
            return true;
        }
        if (DateTime.TryParse(authenticationToken.ExpiryDate, null, DateTimeStyles.RoundtripKind, out var expiryDate))
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

    private async Task SaveState()
    {
        if (authenticationToken == null)
        {
            SecureStorage.Remove(AuthenticationTokenKey);
        }
        else
        {
            string tokenJson = JsonSerializer.Serialize(authenticationToken);
            await SecureStorage.SetAsync(AuthenticationTokenKey, tokenJson).ConfigureAwait(false);
        }
        if (user == null)
        {
            SecureStorage.Remove(PublicKeyKey);
        }
        else
        {
            await SecureStorage.SetAsync(PublicKeyKey, user.publicKey).ConfigureAwait(false);
        }
    }
}