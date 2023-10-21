using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogMaui
{
    public partial class AppShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool loggedIn = true;

        [ObservableProperty]
        private bool loggedOut = false;
    }
}
