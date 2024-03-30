using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BlogMaui.Areas.Blog.Posts;
public partial class PostHeaderViewModel : ObservableObject
{
    private readonly Post post;

    [ObservableProperty]
    private string title = "Uninitialized";

    public ICommand Select { get; }

    public PostHeaderViewModel(Post post)
    {
        Select = new AsyncRelayCommand(HandleSelect);
        this.post = post;
    }

    private async Task HandleSelect()
    {
        Dictionary<string, object> parameters = new()
        {
            { "post", post }
        };
        await Shell.Current.GoToAsync("detail", parameters);
    }
}
