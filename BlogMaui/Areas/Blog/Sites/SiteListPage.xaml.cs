using Jinaga.Maui;

namespace BlogMaui.Areas.Blog.Sites;

public partial class SiteListPage : ContentPage
{
    private readonly SiteListViewModel viewModel;

    public SiteListPage(SiteListViewModel viewModel)
    {
        this.viewModel = viewModel;

        BindingContext = viewModel;

        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    protected override void OnAppearing()
    {
        viewModel.Load();
        base.OnAppearing();
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