using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;
using System.Collections.Immutable;

namespace BlogMaui.Blog;

public partial class PostViewModel : ObservableObject, IQueryAttributable
{
    private readonly JinagaClient jinagaClient;

    [ObservableProperty]
    private string title = "";

    private Post? post;
    private IObserver? titlesObserver = null;
    private ImmutableList<PostTitle> titles = ImmutableList<PostTitle>.Empty;

    private ImmutableList<PostTitle>? frozenTitles;
    private bool editingTitle = false;

    public PostViewModel(JinagaClient jinagaClient)
    {
        this.jinagaClient = jinagaClient;
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
            if (!editingTitle)
            {
                Title = postTitle.value;
            }

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

    public void BeginEditTitle()
    {
        frozenTitles = titles;
        editingTitle = true;
    }

    public void EndEditTitle()
    {
        // Record a new post title if it has changed.
        if (post != null && frozenTitles != null &&
            (frozenTitles.Count != 1 ||
             frozenTitles[0].value != Title))
        {
            jinagaClient.Fact(new PostTitle(post, Title, frozenTitles.ToArray()));
        }

        frozenTitles = null;
        editingTitle = false;
    }
}