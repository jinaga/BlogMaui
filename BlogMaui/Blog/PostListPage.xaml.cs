using Microsoft.Extensions.Logging;

namespace BlogMaui.Blog;

// The refresh view does not use the command to indicate
// that the user has initiated a refresh. It executes the
// command whenever the IsRefreshing property becomes true.
// This causes the command to execute twice.
// Until this is fixed, do not use the RefreshView.
// https://github.com/dotnet/maui/issues/6456

public partial class PostListPage : ContentPage
{
    private readonly PostListViewModel viewModel;
    private readonly ILogger<PostListPage> logger;

    public PostListPage(PostListViewModel viewModel, ILogger<PostListPage> logger)
    {
        this.viewModel = viewModel;
        this.logger = logger;

        BindingContext = viewModel;

        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        logger.LogInformation("OnNavigatedTo PostListPage");
        viewModel.Load("michaelperry.net");
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        logger.LogInformation("OnNavigatedFrom PostListPage");
        if (viewModel != null)
        {
            viewModel.Unload();
        }
        base.OnNavigatedFrom(args);
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        logger.LogInformation("OnNavigatingFrom PostListPage");
        base.OnNavigatingFrom(args);
    }

    protected override void OnAppearing()
    {
        logger.LogInformation("OnAppearing PostListPage");
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        logger.LogInformation("OnDisappearing PostListPage");
        base.OnDisappearing();
    }
}

