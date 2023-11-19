namespace Jinaga.Maui.Authentication;
public class AuthenticationService
{
    private readonly OAuth2HttpAuthenticationProvider authenticationProvider;
    private readonly JinagaClient jinagaClient;

    public AuthenticationService(OAuth2HttpAuthenticationProvider authenticationProvider, JinagaClient jinagaClient)
    {
        this.authenticationProvider = authenticationProvider;
        this.jinagaClient = jinagaClient;
    }

    public async Task<User?> Initialize()
    {
        return await authenticationProvider.Initialize(jinagaClient);
    }

    public async Task<User?> Login()
    {
        return await authenticationProvider.Login(jinagaClient);
    }

    public async Task LogOut()
    {
        await authenticationProvider.LogOut();
    }
}
