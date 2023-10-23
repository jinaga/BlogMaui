using BlogMaui.Authentication;
using BlogMaui.Blog;
using Jinaga;
using Jinaga.Http;
using Jinaga.Store.SQLite;

namespace BlogMaui;
public static class JinagaConfig
{
    public static Settings CreateSettings(IServiceProvider _)
    {
        var settings = new Settings();
        settings.Verify();
        return settings;
    }

    public static OAuth2HttpAuthenticationProvider CreateAuthenticationProvider(IServiceProvider services)
    {
        var settings = services.GetRequiredService<Settings>();

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
        var settings = services.GetRequiredService<Settings>();

        var jinagaClient = JinagaSQLiteClient.Create(opt =>
        {
            opt.HttpEndpoint = new Uri(settings.ReplicatorUrl);
            opt.HttpAuthenticationProvider = authenticationProvider;
            opt.SQLitePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "blog.db");
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
