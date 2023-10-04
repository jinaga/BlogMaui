using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogMaui.Blog;
public partial class PostHeaderViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Uninitialized";
}
