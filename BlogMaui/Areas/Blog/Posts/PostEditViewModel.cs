using BlogMaui.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Windows.Input;

namespace BlogMaui.Areas.Blog.Posts;
public partial class PostEditViewModel : ObservableObject
{
    private ILogger<PostEditViewModel> logger;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool shouldMergeTitles;
    private List<string> titleCandidates;

    public ICommand MergeTitlesCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private readonly JinagaClient jinagaClient;
    private readonly Post post;
    private readonly ImmutableList<PostTitle> titles;

    public PostEditViewModel(JinagaClient jinagaClient, Post post, ImmutableList<PostTitle> titles, ILogger<PostEditViewModel> logger)
    {
        this.jinagaClient = jinagaClient;
        this.post = post;
        this.titles = titles;

        titleCandidates = titles.Select(t => t.value).Order().Distinct().ToList();
        Title = titles
            .Select(t => t.value)
            .Order()
            .FirstOrDefault() ?? string.Empty;
        ShouldMergeTitles = titleCandidates.Count > 1;

        MergeTitlesCommand = new AsyncRelayCommand(HandleMergeTitles);
        SaveCommand = new AsyncRelayCommand(HandleSave);
        CancelCommand = new AsyncRelayCommand(HandleCancel);
        this.logger = logger;
    }

    private async Task HandleMergeTitles()
    {
        logger.LogInformation("Merging titles: {titles}", string.Join(", ", titleCandidates));

        var mergeViewModel = new MergeViewModel(titleCandidates, selection => Title = selection);
        var currentPage = Shell.Current.CurrentPage;
        if (currentPage.Parent is NavigationPage navigationPage)
        {
            await navigationPage.PushAsync(new MergePage(mergeViewModel));
        }
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
