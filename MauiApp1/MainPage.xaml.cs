using BlogMaui.Blog;

namespace BlogMaui;

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

