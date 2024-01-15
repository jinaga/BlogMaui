using Jinaga;

namespace Jinaga.Maui.Authentication;
public class AuthenticationSettings
{
    public AuthenticationSettings(string? authUrl, string? accessTokenUrl, string callbackUrl, string? clientId, string scope, Func<JinagaClient, User, UserProfile, Task> updateUserName)
    {
        AuthUrl = authUrl;
        AccessTokenUrl = accessTokenUrl;
        CallbackUrl = callbackUrl;
        ClientId = clientId;
        Scope = scope;
        UpdateUserName = updateUserName;
    }

    public string? AuthUrl { get; }
    public string? AccessTokenUrl { get; }
    public string CallbackUrl { get; }
    public string? ClientId { get; }
    public string Scope { get; }
    public Func<JinagaClient, User, UserProfile, Task> UpdateUserName { get; }
}
