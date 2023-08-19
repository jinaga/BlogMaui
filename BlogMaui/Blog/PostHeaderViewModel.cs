using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogMaui.Blog;
internal partial class PostHeaderViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Uninitialized";
}
