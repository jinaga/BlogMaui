﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using System.Collections.Immutable;
using System.Windows.Input;

namespace BlogMaui.Areas.Blog.Posts;

public partial class PostViewModel : ObservableObject, IQueryAttributable
{
    private readonly JinagaClient jinagaClient;

    [ObservableProperty]
    private string title = "";

    public ICommand EditCommand { get; }

    private Post? post;
    private IObserver? titlesObserver = null;
    private ImmutableList<PostTitle> titles = ImmutableList<PostTitle>.Empty;

    public PostViewModel(JinagaClient jinagaClient)
    {
        this.jinagaClient = jinagaClient;

        EditCommand = new AsyncRelayCommand(HandleEdit);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        post = query.GetParameter<Post>("post");

        var titlesOfPost = Given<Post>.Match((post, facts) =>
            from title in facts.OfType<PostTitle>()
            where title.post == post &&
                !facts.Any<PostTitle>(next =>
                    next.prior.Contains(title))
            select title
        );

        titlesObserver = jinagaClient.Watch(titlesOfPost, post, postTitle =>
        {
            titles = titles.Add(postTitle);
            Title = postTitle.value;

            return () =>
            {
                titles = titles.Remove(postTitle);
            };
        });
    }

    public void Unload()
    {
        titlesObserver?.Stop();
        titlesObserver = null;
    }

    private async Task HandleEdit()
    {
        if (post != null)
        {
            var viewModel = new PostEditViewModel(jinagaClient, post, titles);
            await Shell.Current.Navigation.PushModalAsync(new PostEditPage(viewModel));
        }
    }
}