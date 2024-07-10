using BlogMaui.Areas.Blog.Posts;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        viewModel.Load();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        if (Shell.Current.Navigation.NavigationStack.Any(page => page == this) ||
            Shell.Current.Navigation.ModalStack.Any(page => page == this))
        {
            return;
        }

        base.OnNavigatedFrom(args);

        viewModel.Unload();
    }
}
