using Jinaga.Http;
using Jinaga.Maui.Authentication;
using Jinaga.Maui.Binding;

namespace Jinaga.Maui;
public static class ConfigurationExtensions
{
    public static IServiceCollection AddJinagaAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<ITokenStorage, SecureTokenStorage>();
        services.AddSingleton<UserProvider>();
        services.AddSingleton<AuthenticationProviderProxy>();
        services.AddSingleton<IHttpAuthenticationProvider>(
            s => s.GetRequiredService<AuthenticationProviderProxy>());
        services.AddSingleton<OAuthClient>();
        services.AddSingleton<AuthenticationService>();
        return services;
    }

    public static IServiceCollection AddJinagaNavigation(this IServiceCollection services, Func<NavigationTreeBuilder, NavigationTreeBuilder> configure)
    {
        services.AddSingleton<INavigationLifecycleManager>(s =>
        {
            var builder = new NavigationTreeBuilder();
            NavigationTree tree = configure(builder).Build();
            return new NavigationLifecycleManager(tree);
        });
        return services;
    }
}
