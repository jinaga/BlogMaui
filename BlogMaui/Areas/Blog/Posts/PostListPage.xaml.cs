using Jinaga.Maui;

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

    public PostListPage(PostListViewModel viewModel)
    {
        this.viewModel = viewModel;

        BindingContext = viewModel;

        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        viewModel.Load();
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        if (this.PageIsInStack())
        {
            return;
        }

        viewModel.Unload();
        base.OnNavigatedFrom(args);
    }
}