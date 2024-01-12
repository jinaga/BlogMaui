using BlogMaui.Areas.Blog.Posts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BlogMaui.Areas.Blog;
public partial class PostEditViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    public ObservableCollection<string> Candidates { get; }
    [ObservableProperty]
    private string selectedCandidate = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private readonly JinagaClient jinagaClient;
    private readonly Post post;
    private readonly ImmutableList<PostTitle> titles;

    public PostEditViewModel(JinagaClient jinagaClient, Post post, ImmutableList<PostTitle> titles)
    {
        this.jinagaClient = jinagaClient;
        this.post = post;
        this.titles = titles;

        Title = titles
            .Select(t => t.value)
            .Order()
            .FirstOrDefault() ?? string.Empty;
        Candidates = new ObservableCollection<string>(titles.Select(t => t.value));

        SaveCommand = new AsyncRelayCommand(HandleSave);
        CancelCommand = new AsyncRelayCommand(HandleCancel);
    }

    partial void OnSelectedCandidateChanged(string value)
    {
        Title = value;
    }

    private async Task HandleSave()
    {
        // Record a new post title if it has changed.
        if (titles.Count != 1 ||
            titles[0].value != Title)
        {
            await jinagaClient.Fact(new PostTitle(post, Title, titles.ToArray()));
        }

        await Shell.Current.Navigation.PopModalAsync();
    }

    private async Task HandleCancel()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
