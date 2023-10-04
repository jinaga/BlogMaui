using BlogMaui.Authentication;
using BlogMaui.Blog;
using Jinaga;
using Jinaga.Http;

namespace BlogMaui;
public static class JinagaConfig
{
    public static OAuth2HttpAuthenticationProvider CreateAuthenticationProvider(IServiceProvider services)
    {
        var settings = new Settings();
        settings.Verify();

        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        var oauth2Client = new OAuthClient(
            settings.AuthUrl,
            settings.AccessTokenUrl,
            settings.CallbackUrl,
            settings.ClientId,
            settings.Scope,
            httpClientFactory);
        var authenticationProvider = new OAuth2HttpAuthenticationProvider(oauth2Client);
        return authenticationProvider;
    }

    public static JinagaClient CreateJinagaClient(IServiceProvider services)
    {
        var authenticationProvider = services.GetRequiredService<IHttpAuthenticationProvider>();
        var settings = new Settings();
        settings.Verify();

        var jinagaClient = JinagaClient.Create(opt =>
        {
            opt.HttpEndpoint = new Uri(settings.ReplicatorUrl);
            opt.HttpAuthenticationProvider = authenticationProvider;
        });
        return jinagaClient;
    }

    public static string Authorization() =>
        AuthorizationRules.Describe(Authorize);

    private static AuthorizationRules Authorize(AuthorizationRules r) => r
        .Any<User>()
        .Type<Site>(site => site.creator)
        .Type<Post>(post => post.site.creator)
        .Type<PostTitle>(title => title.post.site.creator)
        .Type<PostDeleted>(deleted => deleted.post.site.creator)
        .Type<Publish>(publish => publish.post.site.creator);
}
