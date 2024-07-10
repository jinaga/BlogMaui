using Jinaga.Maui;

namespace BlogMaui.Areas.Blog.Posts;

public partial class PostPage : ContentPage
{
    private readonly PostViewModel viewModel;
    private Page? previousPage;

    public PostPage(PostViewModel viewModel)
	{
        this.viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        if (previousPage == null)
        {
            previousPage = args.GetPreviousPage();
            viewModel.Load();
        }
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        if (previousPage == null || args.GetDestinationPage() == previousPage)
        {
            viewModel.Unload();
        }

        base.OnNavigatedFrom(args);
    }
}