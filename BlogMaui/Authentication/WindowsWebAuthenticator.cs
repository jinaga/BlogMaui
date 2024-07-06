namespace BlogMaui.Authentication;

#if WINDOWS
public class WindowsWebAuthenticator : IWebAuthenticator
{
    public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
    {
        if (webAuthenticatorOptions == null)
            throw new ArgumentNullException(nameof(webAuthenticatorOptions));
        if (webAuthenticatorOptions.Url == null)
            throw new ArgumentNullException(nameof(webAuthenticatorOptions.Url));
        if (webAuthenticatorOptions.CallbackUrl == null)
            throw new ArgumentNullException(nameof(webAuthenticatorOptions.CallbackUrl));

        var result = await WinUIEx.WebAuthenticator.AuthenticateAsync(
            webAuthenticatorOptions.Url,
            webAuthenticatorOptions.CallbackUrl);
        return new WebAuthenticatorResult(result.Properties);
    }
}
#endif