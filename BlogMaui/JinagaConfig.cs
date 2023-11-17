using BlogMaui.Blog;
using Jinaga;
using Jinaga.Http;
using Jinaga.Maui.Authentication;
using Jinaga.Store.SQLite;
using MetroLog;
using Microsoft.Extensions.Logging;

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
        var authenticationProvider = new OAuth2HttpAuthenticationProvider(oauth2Client,
            services.GetRequiredService<ILogger<OAuth2HttpAuthenticationProvider>>());
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
    public static string Distribution() =>
        DistributionRules.Describe(Distribute);

    private static AuthorizationRules Authorize(AuthorizationRules r) => r
        .Any<User>()
        .Type<UserName>(name => name.user)
        .Type<Site>(site => site.creator)
        .Type<Post>(post => post.site.creator)
        .Type<PostTitle>(title => title.post.site.creator)
        .Type<PostDeleted>(deleted => deleted.post.site.creator)
        .Type<Publish>(publish => publish.post.site.creator);

    private static DistributionRules Distribute(DistributionRules r) => r
        .Share(Given<User>.Match((user, facts) =>
            from name in facts.OfType<UserName>()
            where name.user == user &&
                !facts.Any<UserName>(next =>
                    next.prior.Contains(name))
            select name
        )).With(Given<User>.Match((user, facts) =>
            from self in facts.OfType<User>()
            where self == user
            select self
        ))
        .Share(Given<User>.Match((user, facts) =>
            from site in facts.OfType<Site>()
            where site.creator == user
            select site
        )).With(Given<User>.Match((user, facts) =>
            from self in facts.OfType<User>()
            where self == user
            select self
        ))
        .Share(Given<Site>.Match((site, facts) =>
            from post in facts.OfType<Post>()
            where post.site == site &&
                !facts.Any<PostDeleted>(deleted =>
                    deleted.post == post)
            select new
            {
                post,
                names =
                    from title in facts.OfType<PostTitle>()
                    where title.post == post &&
                        !facts.Any<PostTitle>(next =>
                            next.prior.Contains(title))
                    select title
            }
        )).With(site => site.creator)
        .Share(Given<Post>.Match((post, facts) =>
            from title in facts.OfType<PostTitle>()
            where title.post == post &&
                !facts.Any<PostTitle>(next =>
                    next.prior.Contains(title))
            select title
        )).With(post => post.site.creator);
}
