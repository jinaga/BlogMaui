using BlogMaui.Areas.Blog;
using CommunityToolkit.Mvvm.ComponentModel;
using Jinaga;

namespace BlogMaui.Areas.Account;
public partial class AccountViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    public string userName = string.Empty;

    private readonly JinagaClient jinagaClient;

    private User? user;
    private IObserver? observer;

    public AccountViewModel(JinagaClient jinagaClient)
    {
        this.jinagaClient = jinagaClient;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        user = query.GetParameter<User>("user");
        Load();
    }

    public void Load()
    {
        if (user == null || observer != null)
        {
            return;
        }

        var namesOfUser = Given<User>.Match((user, facts) =>
            from name in facts.OfType<UserName>()
            where name.user == user &&
                !facts.Any<UserName>(next => next.prior.Contains(name))
            select name.value
        );

        observer = jinagaClient.Watch(namesOfUser, user, projection =>
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
