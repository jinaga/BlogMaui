namespace BlogMaui;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
