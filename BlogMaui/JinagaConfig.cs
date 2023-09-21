using BlogMaui.Authentication;
using BlogMaui.Blog;
using Jinaga;

namespace BlogMaui;
public static class JinagaConfig
{
    public static JinagaClient j = JinagaClient.Create(opt =>
    {
        var settings = new Settings();
        settings.Verify();
        opt.HttpEndpoint = new Uri(settings.ReplicatorUrl);
        var oauth2Client = new OAuthClient(
            settings.AuthUrl,
            settings.AccessTokenUrl,
            settings.CallbackUrl,
            settings.ClientId,
            settings.Scope);
        opt.HttpAuthenticationProvider = new OAuth2HttpAuthenticationProvider(oauth2Client);
    });

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
