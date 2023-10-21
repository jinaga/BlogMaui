﻿namespace BlogMaui.Blog;

// The refresh view does not use the command to indicate
// that the user has initiated a refresh. It executes the
// command whenever the IsRefreshing property becomes true.
// This causes the command to execute twice.
// Until this is fixed, do not use the RefreshView.
// https://github.com/dotnet/maui/issues/6456

public partial class PostListPage : ContentPage
{
    private readonly PostListViewModel viewModel;

    public PostListPage(PostListViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        viewModel.Load("michaelperry.net");
        BindingContext = viewModel;
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        if (viewModel != null)
        {
            viewModel.Unload();
        }
        base.OnDisappearing();
    }
}
