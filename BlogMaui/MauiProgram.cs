﻿using BlogMaui.Account;
using BlogMaui.Authentication;
using BlogMaui.Blog;
using BlogMaui.Visitor;
using CommunityToolkit.Maui;
using Jinaga.Maui;
using Jinaga.Maui.Authentication;
using MetroLog.MicrosoftExtensions;
using MetroLog.Operators;
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

        builder.Logging
            .AddInMemoryLogger(
                options =>
                {
                    options.MaxLines = 1000;
                    options.MinLevel = LogLevel.Debug;
                    options.MaxLevel = LogLevel.Critical;
                })
            .AddStreamingFileLogger(
                options =>
                {
                    options.RetainDays = 2;
                    options.FolderPath = Path.Combine(
                        FileSystem.CacheDirectory,
                        "MetroLogs");
                })
            .AddConsoleLogger(
                options =>
                {
                    options.MinLevel = LogLevel.Information;
                    options.MaxLevel = LogLevel.Critical;
                }); // Will write to the Console Output (logcat for android)

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton(JinagaConfig.CreateSettings);
        builder.Services.AddSingleton(JinagaConfig.CreateOAuthClient);
        builder.Services.AddSingleton<OAuth2HttpAuthenticationProvider>();
        builder.Services.AddSingleton(JinagaConfig.CreateAuthenticationService);
        builder.Services.AddSingleton(JinagaConfig.CreateJinagaClient);
        builder.Services.AddJinagaAuthentication();

        builder.Services.AddSingleton(LogOperatorRetriever.Instance);

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
