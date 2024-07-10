using BlogMaui.Areas.Blog.Posts;
using Jinaga.Maui;

namespace BlogMaui;

public partial class AppShell : Shell
{
    private AppShellViewModel viewModel;

    public AppShell(AppShellViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        BindingContext = viewModel;

        Routing.RegisterRoute("loggedin/posts", typeof(PostListPage));
        Routing.RegisterRoute("loggedin/posts/detail", typeof(PostPage));
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

        base.OnNavigatedFrom(args);

        viewModel.Unload();
    }
}
