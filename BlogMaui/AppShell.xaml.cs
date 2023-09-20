using BlogMaui.Authentication;

namespace BlogMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("gatekeeper", typeof(GatekeeperPage));
        Routing.RegisterRoute("main", typeof(MainPage));
    }
}
