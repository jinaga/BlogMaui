using Jinaga.Maui;

namespace BlogMaui.Areas.Blog.Posts;

public partial class PostPage : ContentPage
{
    private readonly PostViewModel viewModel;

    public PostPage(PostViewModel viewModel)
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