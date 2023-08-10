using MauiApp1.Blog;

namespace MauiApp1;

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
        Task.Run(() => viewModel.Unload());
        base.OnDisappearing();
    }
}

