using BlogMaui.Authentication;
using BlogMaui.Blog;
using Jinaga;

namespace BlogMaui;
public static class JinagaConfig
{
    public static OAuth2HttpAuthenticationProvider AuthenticationProvider { get; }
    public static JinagaClient j { get; }

    static JinagaConfig()
    {
        var settings = new Settings();
        settings.Verify();

        var oauth2Client = new OAuthClient(
            settings.AuthUrl,
            settings.AccessTokenUrl,
            settings.CallbackUrl,
            settings.ClientId,
            settings.Scope);
        AuthenticationProvider = new OAuth2HttpAuthenticationProvider(oauth2Client);

        j = JinagaClient.Create(opt =>
        {
            opt.HttpEndpoint = new Uri(settings.ReplicatorUrl);
            opt.HttpAuthenticationProvider = AuthenticationProvider;
        });
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
