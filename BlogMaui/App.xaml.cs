using BlogMaui.Areas.Blog.Posts;
using MetroLog.Maui;

namespace BlogMaui;

public partial class App : Application
{
    public App(AppShell appShell)
    {
        InitializeComponent();

        MainPage = appShell;

        LogController.InitializeNavigation(
            page => MainPage!.Navigation.PushModalAsync(page),
            () => MainPage!.Navigation.PopModalAsync());

        Routing.RegisterRoute("blog/posts", typeof(PostListPage));
    }
}
