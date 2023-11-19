using Jinaga.Http;
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

    private readonly SemaphoreSlim semaphore = new(1);
    volatile private AuthenticationToken? authenticationToken;
    private User? user;

    public OAuth2HttpAuthenticationProvider(OAuthClient oauthClient, Func<JinagaClient, User, UserProfile, Task> updateUserName)
    {
        this.oauthClient = oauthClient;
        this.updateUserName = updateUserName;
    }

    public async Task<User?> Initialize(JinagaClient jinagaClient)
    {
        await Lock(async () =>
        {
            await LoadToken().ConfigureAwait(false);
            if (authenticationToken != null)
            {
                // Check for token expiration
                if (DateTime.TryParse(authenticationToken.ExpryDate, null, DateTimeStyles.RoundtripKind, out var expiryDate))
                {
                    if (DateTime.UtcNow > expiryDate.AddMinutes(-5))
                    {
                        await RefreshToken().ConfigureAwait(false);
                        await SaveToken().ConfigureAwait(false);
                    }
                }
            }
            await LoadUser().ConfigureAwait(false);
            return true;
        }).ConfigureAwait(false);
        return await GetUser(jinagaClient).ConfigureAwait(false);
    }

    public Task<User?> Login(JinagaClient jinagaClient)
    {
        return Lock(async () =>
        {
            var client = oauthClient;
            string requestUrl = client.BuildRequestUrl();
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(requestUrl),
                new Uri(client.CallbackUrl)).ConfigureAwait(false);
            if (authResult == null)
            {
                return null;
            }

            string state = authResult.Properties["state"];
            string code = authResult.Properties["code"];

            client.ValidateState(state);
            var tokenResponse = await client.GetTokenResponse(code).ConfigureAwait(false);
            authenticationToken = ResponseToToken(tokenResponse);
            await SaveToken().ConfigureAwait(false);
            return await GetUser(jinagaClient).ConfigureAwait(false);
        });
    }

    public async Task LogOut()
    {
        await Lock(async () =>
        {
            authenticationToken = null;
            await SaveToken().ConfigureAwait(false);
            await ClearUser().ConfigureAwait(false);
            return true;
        }).ConfigureAwait(false);
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
        return Lock(async () =>
        {
            if (authenticationToken != null)
            {
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
                var (user, profile) = await jinagaClient.Login().ConfigureAwait(false);

                if (user != null)
                {
                    this.user = user;
                    await SaveUser().ConfigureAwait(false);

                    await updateUserName(jinagaClient, user, profile).ConfigureAwait(false);
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
            await SaveUser().ConfigureAwait(false);
            return true;
        });
    }

    private async Task LoadToken()
    {
        string? tokenJson = await SecureStorage.GetAsync(AuthenticationTokenKey).ConfigureAwait(false);
        if (tokenJson != null)
        {
            AuthenticationToken? authenticationToken = JsonSerializer.Deserialize<AuthenticationToken>(tokenJson);
            this.authenticationToken = authenticationToken;
        }
    }

    private async Task SaveToken()
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
    }

    private async Task RefreshToken()
    {
        if (authenticationToken == null)
        {
            throw new InvalidOperationException("Attempting to refresh with no token");
        }

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
        string? publicKey = await SecureStorage.GetAsync(PublicKeyKey).ConfigureAwait(false);
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
            await SecureStorage.SetAsync(PublicKeyKey, user.publicKey).ConfigureAwait(false);
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