using Jinaga.Http;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Jinaga.Maui.Authentication;

public class OAuth2HttpAuthenticationProvider : IHttpAuthenticationProvider
{
    private const string AuthenticationTokenKey = "Jinaga.AuthenticationToken";

    private readonly OAuthClient oauthClient;

    private readonly SemaphoreSlim semaphore = new(1);
    volatile private AuthenticationToken? authenticationToken;

    internal bool IsLoggedIn => authenticationToken != null;

    public OAuth2HttpAuthenticationProvider(OAuthClient oauthClient)
    {
        this.oauthClient = oauthClient;
    }

    internal Task Initialize()
    {
        return Lock(async () =>
        {
            await LoadToken().ConfigureAwait(false);
            if (authenticationToken != null)
            {
                // Check for token expiration
                if (DateTime.TryParse(authenticationToken.ExpiryDate, null, DateTimeStyles.RoundtripKind, out var expiryDate))
                {
                    if (DateTime.UtcNow > expiryDate.AddMinutes(-5))
                    {
                        try
                        {
                            await RefreshToken().ConfigureAwait(false);
                            await SaveToken().ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                            // We might be offline.
                            // We'll get a new token when we try to use it online.
                            // Don't prevent the user from using the app.
                        }
                    }
                }
            }
            return true;
        });
    }

    internal Task<bool> Login(string provider)
    {
        return Lock(async () =>
        {
            var client = oauthClient;
            string requestUrl = client.BuildRequestUrl(provider);
#if WINDOWS
            var authResult = await WinUIEx.WebAuthenticator.AuthenticateAsync(
                new Uri(requestUrl),
                new Uri(client.CallbackUrl)).ConfigureAwait(false);
#else
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(requestUrl),
                new Uri(client.CallbackUrl)).ConfigureAwait(false);
#endif
            if (authResult == null)
            {
                return false;
            }

            string state = authResult.Properties["state"];
            string code = authResult.Properties["code"];

            client.ValidateState(state);
            var tokenResponse = await client.GetTokenResponse(code).ConfigureAwait(false);
            authenticationToken = ResponseToToken(tokenResponse);
            await SaveToken().ConfigureAwait(false);
            return true;
        });
    }

    internal Task LogOut()
    {
        return Lock(async () =>
        {
            authenticationToken = null;
            await SaveToken().ConfigureAwait(false);
            return true;
        });
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
            ExpiryDate = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                .ToString("O", CultureInfo.InvariantCulture)
        };
    }
}