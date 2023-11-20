using BlogMaui.Areas.Blog;
using BlogMaui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;

namespace BlogMaui.Areas.Account;
public partial class AccountViewModel : ObservableObject
{
    [ObservableProperty]
    public string userName = string.Empty;

    private readonly JinagaClient jinagaClient;
    private readonly UserProvider userProvider;

    private IObserver? observer;

    public AccountViewModel(JinagaClient jinagaClient, UserProvider userProvider)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;
    }

    public void Load()
    {
        if (userProvider.User == null || observer != null)
        {
            return;
        }

        var namesOfUser = Given<User>.Match((user, facts) =>
            from name in facts.OfType<UserName>()
            where name.user == user &&
                !facts.Any<UserName>(next => next.prior.Contains(name))
            select name.value
        );

        observer = jinagaClient.Watch(namesOfUser, userProvider.User, projection =>
        {
            UserName = projection;
        });

        Monitor(observer);
    }

    private void Monitor(IObserver observer)
    {
        observer.Loaded.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                UserName = "Failed to load user information";
            }
            else if (string.IsNullOrEmpty(UserName))
            {
                UserName = "User name unknown";
            }
        });
    }

    public void Unload()
    {
        observer?.Stop();
        observer = null;
    }
}
