using BlogMaui.Authentication;
using BlogMaui.Blog;
using CommunityToolkit.Maui;
using Jinaga.Http;
using Microsoft.Extensions.Logging;

namespace BlogMaui;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton(JinagaConfig.CreateSettings);
        builder.Services.AddSingleton(JinagaConfig.CreateAuthenticationProvider);
        builder.Services.AddSingleton(JinagaConfig.CreateJinagaClient);
        builder.Services.AddSingleton<IHttpAuthenticationProvider>(
            services => services.GetRequiredService<OAuth2HttpAuthenticationProvider>());
        builder.Services.AddSingleton<UserProvider>();

        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<PostListViewModel>();
        builder.Services.AddTransient<GatekeeperPage>();
        builder.Services.AddTransient<GatekeeperViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
