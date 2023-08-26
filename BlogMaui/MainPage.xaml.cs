using BlogMaui.Blog;

namespace BlogMaui;

// The refresh view does not use the command to indicate
// that the user has initiated a refresh. It executes the
// command whenever the IsRefreshing property becomes true.
// This causes the command to execute twice.
// Until this is fixed, do not use the RefreshView.
// https://github.com/dotnet/maui/issues/6456

public partial class MainPage : ContentPage
{
    readonly PostListViewModel viewModel = new();

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        viewModel.Load("qedcode.com");
        BindingContext = viewModel;
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        viewModel.Unload();
        base.OnDisappearing();
    }
}

