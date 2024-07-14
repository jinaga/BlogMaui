using System.ComponentModel;
using BlogMaui.Areas.Blog.Posts;
using Microsoft.Extensions.Logging;

namespace BlogMaui;

public partial class AppShell : Shell
{
    private readonly AppShellViewModel viewModel;
    private readonly ILogger<AppShell> logger;

    public AppShell(AppShellViewModel viewModel, ILogger<AppShell> logger)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        this.logger = logger;
        BindingContext = viewModel;

        Routing.RegisterRoute("loggedin/posts", typeof(PostListPage));
        Routing.RegisterRoute("loggedin/posts/detail", typeof(PostPage));
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        viewModel.Load();
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        viewModel.Unload();
        viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(AppShellViewModel.AppState))
        {
            // When logging out, clear the navigation stack in the home tab.
            if (viewModel.AppState == "NotLoggedIn")
            {
                HomeTab.Navigation.PopToRootAsync()
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            logger.LogError(t.Exception, "Failed to pop to root");
                        }
                    });
            }
        }
    }
}
