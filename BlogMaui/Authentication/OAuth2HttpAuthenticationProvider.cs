using Jinaga.Http;
using System.Net.Http.Headers;

namespace BlogMaui;

internal class OAuth2HttpAuthenticationProvider : IHttpAuthenticationProvider
{
    private string? accessToken;
    private SemaphoreSlim semaphore = new(1);

    public Task SetRequestHeaders(HttpRequestHeaders headers)
    {
        return semaphore.WaitAsync().ContinueWith(_ =>
        {
            if (accessToken == null)
            {
                return Task.CompletedTask;
            }
            else
            {
                headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return Task.CompletedTask;
            }
        });
    }

    public Task<bool> Reauthenticate()
    {
        return Task.FromResult(false);
    }
}