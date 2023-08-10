using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiApp1.Blog;
internal partial class PostHeaderViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "Uninitialized";
}
