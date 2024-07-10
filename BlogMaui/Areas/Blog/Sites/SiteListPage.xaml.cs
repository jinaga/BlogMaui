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
        viewModel.Load();
        base.OnNavigatedTo(args);
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