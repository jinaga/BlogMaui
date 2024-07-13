using BlogMaui.Areas.Blog;
using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;
using Jinaga.Maui.Authentication;
using Jinaga.Maui.Binding;

namespace BlogMaui.Areas.Account;
public partial class AccountViewModel : ObservableObject, ILifecycleManaged
{
    [ObservableProperty]
    public string userName = string.Empty;

    private readonly JinagaClient jinagaClient;
    private readonly UserProvider userProvider;

    private UserProvider.Handler? handler;

    public AccountViewModel(JinagaClient jinagaClient, UserProvider userProvider)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;
    }

    public void Load()
    {
        if (handler != null)
        {
            return;
        }

        handler = userProvider.AddHandler(user =>
        {
            var namesOfUser = Given<User>.Match((user, facts) =>
                from name in facts.OfType<UserName>()
                where name.user == user &&
                    !facts.Any<UserName>(next => next.prior.Contains(name))
                select name.value
            );

            var observer = jinagaClient.Watch(namesOfUser, user, projection =>
            {
                UserName = projection;
            });

            Monitor(observer);

            return () =>
            {
                observer.Stop();
                observer = null;
                UserName = string.Empty;
            };
        });
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
        if (handler != null)
        {
            userProvider.RemoveHandler(handler);
            handler = null;
        }
    }
}
