using BlogMaui.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using System.Collections.Immutable;
using System.Windows.Input;

namespace BlogMaui.Areas.Blog.Posts;
public partial class PostNewViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private readonly JinagaClient jinagaClient;
    private readonly Site site;
    private readonly User author;

    public PostNewViewModel(JinagaClient jinagaClient, Site site, User author)
    {
        this.jinagaClient = jinagaClient;
        this.site = site;
        this.author = author;

        SaveCommand = new AsyncRelayCommand(HandleSave);
        CancelCommand = new AsyncRelayCommand(HandleCancel);
    }

    private async Task HandleSave()
    {
        var post = await jinagaClient.Fact(new Post(site, author, DateTime.UtcNow));
        await jinagaClient.Fact(new PostTitle(post, Title, []));

        await Shell.Current.Navigation.PopModalAsync();
    }

    private async Task HandleCancel()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}