using Jinaga.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Jinaga.Maui.Authentication;

public class OAuth2HttpAuthenticationProvider : IHttpAuthenticationProvider
{
    private const string AuthenticationTokenKey = "BlogMaui.AuthenticationToken";
    private const string PublicKeyKey = "BlogMaui.PublicKey";

    private readonly OAuthClient oauthClient;
    private readonly Func<JinagaClient, User, UserProfile, Task> updateUserName;
    private readonly ILogger<OAuth2HttpAuthenticationProvider> logger;

    private readonly SemaphoreSlim semaphore = new(1);
    volatile private AuthenticationToken? authenticationToken;
    private User? user;

    public OAuth2HttpAuthenticationProvider(OAuthClient oauthClient, Func<JinagaClient, User, UserProfile, Task> updateUserName, ILogger<OAuth2HttpAuthenticationProvider> logger)
    {
        this.oauthClient = oauthClient;
        this.updateUserName = updateUserName;
        this.logger = logger;
    }

    public async Task<User?> Initialize(JinagaClient jinagaClient)
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
        await LoadUser();
        return await GetUser(jinagaClient);
    }

    public Task<User?> Login(JinagaClient jinagaClient)
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
                return null;
            }

            string state = authResult.Properties["state"];
            string code = authResult.Properties["code"];

            client.ValidateState(state);
            var tokenResponse = await client.GetTokenResponse(code);
            authenticationToken = ResponseToToken(tokenResponse);
            await SaveToken();
            return await GetUser(jinagaClient);
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
        await ClearUser();
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

    public Task<User?> GetUser(JinagaClient jinagaClient)
    {
        return Lock(async () =>
        {
            if (authenticationToken == null)
            {
                return null;
            }

            if (this.user == null)
            {
                // Get the logged in user.
                var (user, profile) = await jinagaClient.Login();

                if (user != null)
                {
                    this.user = user;
                    await SaveUser();

                    await updateUserName(jinagaClient, user, profile);
                }
            }
            return user;
        });
    }

    public Task ClearUser()
    {
        return Lock(async () =>
        {
            this.user = null;
            await SaveUser();
            return 0;
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

    private async Task LoadUser()
    {
        string? publicKey = await SecureStorage.GetAsync(PublicKeyKey);
        if (publicKey != null)
        {
            this.user = new User(publicKey);
        }
    }

    private async Task SaveUser()
    {
        if (user == null)
        {
            SecureStorage.Remove(PublicKeyKey);
        }
        else
        {
            await SecureStorage.SetAsync(PublicKeyKey, user.publicKey);
        }
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
}