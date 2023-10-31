using BlogMaui.Account;
using BlogMaui.Authentication;
using BlogMaui.Blog;
using BlogMaui.Visitor;
using CommunityToolkit.Maui;
using Jinaga.Maui;
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
                fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentSystemIcons");
            });

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton(JinagaConfig.CreateSettings);
        builder.Services.AddSingleton(JinagaConfig.CreateAuthenticationProvider);
        builder.Services.AddSingleton(JinagaConfig.CreateJinagaClient);
        builder.Services.AddJinagaAuthentication();
        builder.Services.AddSingleton<UserProvider>();

        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<GatekeeperPage>();
        builder.Services.AddTransient<GatekeeperViewModel>();

        builder.Services.AddTransient<VisitorPage>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<PostListPage>();
        builder.Services.AddTransient<PostListViewModel>();
        builder.Services.AddTransient<PostPage>();
        builder.Services.AddTransient<PostViewModel>();
        builder.Services.AddTransient<AccountPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
