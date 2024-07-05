using System.Collections.Immutable;
using Jinaga;

namespace BlogMaui.Authentication;

public class UserProvider
{
    public class Handler
    {
        public Func<User, Action> WithUser { get; }
        public Action Clear { get; set; } = () => { };

        public Handler(Func<User, Action> withUser)
        {
            WithUser = withUser;
        }
    }

    private User? user;
    private ImmutableList<Handler> handlers = ImmutableList<Handler>.Empty;

    public void SetUser(User user)
    {
        BeforeSetUser();
        this.user = user;
        AfterSetUser();
    }

    public void ClearUser()
    {
        BeforeSetUser();
        user = null;
        AfterSetUser();
    }

    public Handler AddHandler(Func<User, Action> withUser)
    {
        var handler = new Handler(withUser);
        handlers = handlers.Add(handler);
        if (user != null)
        {
            handler.Clear = handler.WithUser(user);
        }
        return handler;
    }

    public void RemoveHandler(Handler handler)
    {
        handlers = handlers.Remove(handler);
        if (user != null)
        {
            handler.Clear();
        }
    }

    private void BeforeSetUser()
    {
        if (user != null)
        {
            foreach (var handler in handlers)
            {
                handler.Clear();
                handler.Clear = () => { };
            }
        }
    }

    private void AfterSetUser()
    {
        if (user != null)
        {
            foreach (var handler in handlers)
            {
                handler.Clear = handler.WithUser(user);
            }
        }
    }
}
