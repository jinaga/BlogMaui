using Jinaga.Http;
using System.Net.Http.Headers;

namespace BlogMaui;

internal class OAuth2HttpAuthenticationProvider : IHttpAuthenticationProvider
{
#pragma warning disable CS0649 // Field 'OAuth2HttpAuthenticationProvider.accessToken' is never assigned to, and will always have its default value null
    private string? accessToken;
#pragma warning restore CS0649 // Field 'OAuth2HttpAuthenticationProvider.accessToken' is never assigned to, and will always have its default value null
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