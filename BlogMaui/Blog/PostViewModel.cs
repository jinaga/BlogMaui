using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;
using System.Collections.Immutable;

namespace BlogMaui.Blog;

public partial class PostViewModel : ObservableObject, IQueryAttributable
{
    private readonly JinagaClient jinagaClient;

    [ObservableProperty]
    private string title = "";

    private IObserver? titlesObserver = null;
    private ImmutableList<PostTitle> titles = ImmutableList<PostTitle>.Empty;

    public PostViewModel(JinagaClient jinagaClient)
    {
        this.jinagaClient = jinagaClient;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var post = query.GetParameter<Post>("post");

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
    }
}