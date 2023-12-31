﻿using BlogMaui.Areas.Blog;
using Jinaga;
using Jinaga.Http;
using Jinaga.Maui.Authentication;
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

    public static AuthenticationSettings CreateAuthenticationSettings(IServiceProvider services)
    {
        var settings = services.GetRequiredService<Settings>();

        var authenticationSettings = new AuthenticationSettings(
            settings.AuthUrl,
            settings.AccessTokenUrl,
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
