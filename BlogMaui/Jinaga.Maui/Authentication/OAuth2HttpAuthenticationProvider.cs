using BlogMaui.Authentication;
using Jinaga.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Jinaga.Maui.Authentication;

public class OAuth2HttpAuthenticationProvider : IHttpAuthenticationProvider
{
    private const string AuthenticationTokenKey = "BlogMaui.AuthenticationToken";

    private readonly UserProvider userProvider;
    private readonly OAuthClient oauthClient;
    private readonly ILogger<OAuth2HttpAuthenticationProvider> logger;

    private readonly SemaphoreSlim semaphore = new(1);
    volatile private AuthenticationToken? authenticationToken;

    public OAuth2HttpAuthenticationProvider(UserProvider userProvider, OAuthClient oauthClient, ILogger<OAuth2HttpAuthenticationProvider> logger)
    {
        this.userProvider = userProvider;
        this.oauthClient = oauthClient;
        this.logger = logger;
    }

    public async Task<bool> Initialize()
    {
        bool loggedIn = await Lock(async () =>
        {
            logger.LogInformation("Initializing OAuth2 provider");
            await LoadToken().ConfigureAwait(false);
            if (authenticationToken != null)
            {
                logger.LogInformation("Loaded a token");
                // Check for token expiration
                if (DateTime.TryParse(authenticationToken.ExpryDate, null, DateTimeStyles.RoundtripKind, out var expiryDate))
                {
                    if (DateTime.UtcNow > expiryDate.AddMinutes(-5))
                    {
                        logger.LogInformation("It was expired");
                        await RefreshToken().ConfigureAwait(false);
                        await SaveToken().ConfigureAwait(false);
                    }
                }
            }
            logger.LogInformation(authenticationToken != null
                ? "Logged in"
                : "Not logged in");
            return authenticationToken != null;
        });
        await userProvider.Initialize();
        return loggedIn;
    }

    public Task<bool> Login()
    {
        return Lock(async () =>
        {
            var client = oauthClient;
            string requestUrl = client.BuildRequestUrl();
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(requestUrl),
                new Uri(client.CallbackUrl));
            if (authResult == null)
            {
                return false;
            }

            string state = authResult.Properties["state"];
            string code = authResult.Properties["code"];

            client.ValidateState(state);
            var tokenResponse = await client.GetTokenResponse(code);
            authenticationToken = ResponseToToken(tokenResponse);
            await SaveToken();
            return true;
        });
    }

    public async Task LogOut()
    {
        await Lock(async () =>
        {
            authenticationToken = null;
            await SaveToken();
            return true;
        });
        await userProvider.ClearUser();
    }

    public void SetRequestHeaders(HttpRequestHeaders headers)
    {
        var cachedAuthenticationToken = authenticationToken;
        if (cachedAuthenticationToken != null)
        {
            logger.LogInformation("Setting authorization header");
            headers.Authorization = new AuthenticationHeaderValue("Bearer", cachedAuthenticationToken.AccessToken);
        }
        else
        {
            logger.LogInformation("No authentication token");
        }
    }

    public Task<bool> Reauthenticate()
    {
        return Lock(async () =>
        {
            if (authenticationToken != null)
            {
                logger.LogInformation("I have an old authentication token");
                await RefreshToken().ConfigureAwait(false);
                await SaveToken().ConfigureAwait(false);
                return true;
            }
            return false;
        });
    }

    private async Task LoadToken()
    {
        string? tokenJson = await SecureStorage.GetAsync(AuthenticationTokenKey).ConfigureAwait(false);
        if (tokenJson != null)
        {
            logger.LogInformation("Loaded a token");
            AuthenticationToken? authenticationToken = JsonSerializer.Deserialize<AuthenticationToken>(tokenJson);
            this.authenticationToken = authenticationToken;
        }
        else
        {
            logger.LogInformation("There was no token in storage");
        }
    }

    private async Task SaveToken()
    {
        if (authenticationToken == null)
        {
            logger.LogInformation("Clearing the token");
            SecureStorage.Remove(AuthenticationTokenKey);
        }
        else
        {
            logger.LogInformation("Saving a token");
            string tokenJson = JsonSerializer.Serialize(authenticationToken);
            await SecureStorage.SetAsync(AuthenticationTokenKey, tokenJson).ConfigureAwait(false);
        }
    }

    private async Task RefreshToken()
    {
        if (authenticationToken == null)
        {
            throw new InvalidOperationException("Attempting to refresh with no token");
        }

        logger.LogInformation("Refreshing token");
        var tokenResponse = await oauthClient.RequestNewToken(authenticationToken.RefreshToken).ConfigureAwait(false);
        authenticationToken = ResponseToToken(tokenResponse);
    }

    private async Task<T> Lock<T>(Func<Task<T>> action)
    {
        await semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await action().ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static AuthenticationToken ResponseToToken(TokenResponse tokenResponse)
    {
        return new AuthenticationToken
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpryDate = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                .ToString("O", CultureInfo.InvariantCulture)
        };
    }

    public async Task<User?> GetUser(JinagaClient jinagaClient, bool loggedIn)
    {
        return loggedIn ? await userProvider.GetUser(jinagaClient) : null;
    }
}