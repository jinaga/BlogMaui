using Microsoft.Extensions.Logging;

namespace BlogMaui.Areas.Blog.Posts;

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

    protected override void OnDisappearing()
    {
        logger.LogInformation("OnDisappearing PostListPage");
        viewModel.Unload();
        base.OnDisappearing();
    }
}