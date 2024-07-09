using BlogMaui.Areas.Account;
using BlogMaui.Areas.Blog.Posts;
using BlogMaui.Areas.Blog.Sites;
using BlogMaui.Areas.Visitor;
using BlogMaui.Authentication;
using CommunityToolkit.Maui;
using Jinaga.Maui;
using Jinaga.Maui.Binding;
using MetroLog.MicrosoftExtensions;
using MetroLog.Operators;
using Microsoft.Extensions.Logging;

[assembly:XamlCompilation(XamlCompilationOptions.Compile)]

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

        builder.Logging
            .SetMinimumLevel(LogLevel.Information)
            .AddTraceLogger(
                options =>
                {
                    options.MinLevel = LogLevel.Trace;
                    options.MaxLevel = LogLevel.Critical;
                }) // Will write to the Debug Output
            .AddInMemoryLogger(
                options =>
                {
                    options.MaxLines = 1000;
                    options.MinLevel = LogLevel.Trace;
                    options.MaxLevel = LogLevel.Critical;
                })
            .AddStreamingFileLogger(
                options =>
                {
                    options.RetainDays = 2;
                    options.FolderPath = Path.Combine(
                        FileSystem.CacheDirectory,
                        "BlogMauiLogs");
                })
            .AddConsoleLogger(
                options =>
                {
                    options.MinLevel = LogLevel.Trace;
                    options.MaxLevel = LogLevel.Critical;
                }); // Will write to the Console Output (logcat for android)

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton(JinagaConfig.CreateSettings);
        builder.Services.AddSingleton(JinagaConfig.CreateAuthenticationSettings);
        builder.Services.AddSingleton(JinagaConfig.CreateJinagaClient);
#if WINDOWS
        builder.Services.AddSingleton<IWebAuthenticator, WindowsWebAuthenticator>();
#else
        builder.Services.AddSingleton(WebAuthenticator.Default);
#endif
        builder.Services.AddJinagaAuthentication();

        builder.Services.AddSingleton(LogOperatorRetriever.Instance);

        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<GatekeeperPage>();
        builder.Services.AddTransient<GatekeeperViewModel>();

        builder.Services.AddTransient<VisitorPage>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<SiteListPage>();
        builder.Services.AddTransient<SiteListViewModel>();

        builder.Services.AddTransient<PostListPage>();
        builder.Services.AddTransient<PostListViewModel>();
        builder.Services.AddTransient<PostPage>();
        builder.Services.AddTransient<PostViewModel>();
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<AccountViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
