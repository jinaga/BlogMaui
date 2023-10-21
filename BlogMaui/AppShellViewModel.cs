using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogMaui
{
    public partial class AppShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private string appState = "Initializing";
    }
}
