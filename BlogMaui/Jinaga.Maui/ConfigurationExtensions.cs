using Jinaga.Http;
using Jinaga.Maui.Authentication;

namespace Jinaga.Maui;
public static class ConfigurationExtensions
{
    public static IServiceCollection AddJinagaAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<IHttpAuthenticationProvider>(
            s => s.GetRequiredService<AuthenticationService>());
        services.AddSingleton<OAuthClient>();
        services.AddSingleton<AuthenticationService>();
        return services;
    }
}
