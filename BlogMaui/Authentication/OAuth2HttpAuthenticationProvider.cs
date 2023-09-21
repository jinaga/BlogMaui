using BlogMaui.Authentication;
using Jinaga.Http;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BlogMaui;

internal class OAuth2HttpAuthenticationProvider : IHttpAuthenticationProvider
{
    private const string AuthenticationTokenKey = "BlogMaui.AuthenticationToken";

    private readonly OAuthClient oauthClient;

    private SemaphoreSlim semaphore = new(1);
    volatile private AuthenticationToken? authenticationToken;

    public OAuth2HttpAuthenticationProvider(OAuthClient oauthClient)
    {
        this.oauthClient = oauthClient;
    }

    public Task Initialize()
    {
        return Lock(async () =>
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

    private async Task<bool> LoadToken()
    {
        string tokenJson = await SecureStorage.GetAsync(AuthenticationTokenKey).ConfigureAwait(false);
        AuthenticationToken? authenticationToken = JsonSerializer.Deserialize<AuthenticationToken>(tokenJson);
        this.authenticationToken = authenticationToken;
        return authenticationToken != null;
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
        authenticationToken = new AuthenticationToken
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpryDate = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                .ToString("O", CultureInfo.InvariantCulture)
        };
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