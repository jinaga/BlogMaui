using System.Collections.Immutable;
using BlogMaui.Areas.Blog;
using Jinaga;
using Jinaga.Http;
using Jinaga.Maui.Authentication;
using Jinaga.Store.SQLite;
using Microsoft.Extensions.Logging;

namespace BlogMaui;
public static class JinagaConfig
{
    public static Settings CreateSettings(IServiceProvider _)
    {
        var settings = new Settings();
        return settings;
    }

    public static AuthenticationSettings CreateAuthenticationSettings(IServiceProvider services)
    {
        var settings = services.GetRequiredService<Settings>();

        var authUrlByProvider = ImmutableDictionary<string, string>.Empty;
        if (settings.AppleAuthUrl != null)
        {
            authUrlByProvider = authUrlByProvider.Add("Apple", settings.AppleAuthUrl);
        }
        if (settings.GoogleAuthUrl != null)
        {
            authUrlByProvider = authUrlByProvider.Add("Google", settings.GoogleAuthUrl);
        }
        if (settings.AccessTokenUrl == null)
        {
            throw new Exception("No access token URL is configured.");
        }
        if (settings.RevokeUrl == null)
        {
            throw new Exception("No revoke URL is configured.");
        }
        if (settings.ClientId == null)
        {
            throw new Exception("No client ID is configured.");
        }

        var authenticationSettings = new AuthenticationSettings(
            authUrlByProvider,
            settings.AccessTokenUrl,
            settings.RevokeUrl,
            settings.CallbackUrl,
            settings.ClientId,
            settings.Scope,
            UpdateUserName);
        return authenticationSettings;
    }

    private static async Task UpdateUserName(JinagaClient jinagaClient, User user, UserProfile profile)
    {
        // Load the current user name.
        var userNames = await jinagaClient.Query(Given<User>.Match((user, facts) =>
            from name in facts.OfType<UserName>()
            where name.user == user &&
                !facts.Any<UserName>(next => next.prior.Contains(name))
            select name
        ), user);

        // If the name is different, then update it.
        if (userNames.Count != 1 || userNames.Single().value != profile.DisplayName)
        {
            await jinagaClient.Fact(new UserName(user, profile.DisplayName, userNames.ToArray()));
        }
    }

    public static JinagaClient CreateJinagaClient(IServiceProvider services)
    {
        var authenticationProvider = services.GetRequiredService<IHttpAuthenticationProvider>();
        var settings = services.GetRequiredService<Settings>();

        var jinagaClient = JinagaSQLiteClient.Create(opt =>
        {
            if (settings.ReplicatorUrl != null)
            {
                opt.HttpEndpoint = new Uri(settings.ReplicatorUrl);
            }
            opt.HttpAuthenticationProvider = authenticationProvider;
            opt.SQLitePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "blog.db");
            opt.LoggerFactory = services.GetRequiredService<ILoggerFactory>();
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
        .Type<SiteName>(name => name.site.creator)
        .Type<SiteDomain>(domain => domain.site.creator)
        .Type<Post>(post => post.site.creator)
        .Type<PostTitle>(title => title.post.site.creator)
        .Type<PostDeleted>(deleted => deleted.post.site.creator)
        .Type<Publish>(publish => publish.post.site.creator);

    private static DistributionRules Distribute(DistributionRules r) => r
        // Distribute user names to the user themselves.
        .Share(Given<User>.Match((user, facts) =>
            from name in facts.OfType<UserName>()
            where name.user == user &&
                !facts.Any<UserName>(next =>
                    next.prior.Contains(name))
            select name
        )).With(user => user)

        // Distribute sites with names and domains to the site creator.
        .Share(Given<User>.Match((user, facts) =>
            from site in facts.OfType<Site>()
            where site.creator == user &&
                !facts.Any<SiteDeleted>(deleted =>
                    deleted.site == site &&
                        !facts.Any<SiteRestored>(restored =>
                            restored.deleted == deleted))
            select new {
                site,
                names =
                    from name in facts.OfType<SiteName>()
                    where name.site == site &&
                        !facts.Any<SiteName>(next =>
                            next.prior.Contains(name))
                    select name,
                domains =
                    from domain in facts.OfType<SiteDomain>()
                    where domain.site == site &&
                        !facts.Any<SiteDomain>(next =>
                            next.prior.Contains(domain))
                    select domain
            }
        )).With(user => user)

        // Distribute site names to the site creator.
        .Share(Given<Site>.Match((site, facts) =>
            from name in facts.OfType<SiteName>()
            where name.site == site &&
                !facts.Any<SiteName>(next =>
                    next.prior.Contains(name))
            select name
        )).With(site => site.creator)

        // Distribute site domains to the site creator.
        .Share(Given<Site>.Match((site, facts) =>
            from domain in facts.OfType<SiteDomain>()
            where domain.site == site &&
                !facts.Any<SiteDomain>(next =>
                    next.prior.Contains(domain))
            select domain
        )).With(site => site.creator)

        // Distribute posts with titles to the site creator.
        .Share(Given<Site>.Match((site, facts) =>
            from post in facts.OfType<Post>()
            where post.site == site &&
                !facts.Any<PostDeleted>(deleted =>
                    deleted.post == post &&
                        !facts.Any<PostRestored>(restored =>
                            restored.deleted == deleted))
            select new
            {
                post,
                titles =
                    from title in facts.OfType<PostTitle>()
                    where title.post == post &&
                        !facts.Any<PostTitle>(next =>
                            next.prior.Contains(title))
                    select title
            }
        )).With(site => site.creator)

        // Distribute post titles to the site creator.
        .Share(Given<Post>.Match((post, facts) =>
            from title in facts.OfType<PostTitle>()
            where title.post == post &&
                !facts.Any<PostTitle>(next =>
                    next.prior.Contains(title))
            select title
        )).With(post => post.site.creator);
}
