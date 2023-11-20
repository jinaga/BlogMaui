using BlogMaui.Areas.Blog;

namespace BlogMaui;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        Routing.RegisterRoute("loggedin/posts/detail", typeof(PostPage));
    }
}
