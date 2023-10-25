using Jinaga.Http;
using Jinaga.Maui.Authentication;

namespace Jinaga.Maui;
public static class ConfigurationExtensions
{
    public static IServiceCollection AddJinagaAuthentication(this IServiceCollection services)
    {
        return services.AddSingleton<IHttpAuthenticationProvider>(
            s => s.GetRequiredService<OAuth2HttpAuthenticationProvider>());
    }
}
