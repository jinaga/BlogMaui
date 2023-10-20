using BlogMaui.Blog;
using Jinaga;

namespace BlogMaui.Authentication;
public class UserProvider
{
    private readonly JinagaClient jinagaClient;
    private readonly SemaphoreSlim semaphore = new(1);
    private User? user;

    public UserProvider(JinagaClient jinagaClient)
    {
        this.jinagaClient = jinagaClient;
    }

    public async Task ClearUser()
    {
        await semaphore.WaitAsync();
        try
        {
            this.user = null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<User?> GetUser()
    {
        await semaphore.WaitAsync();
        try
        {
            if (this.user == null)
            {
                // Get the logged in user.
                var (user, profile) = await jinagaClient.Login();

                if (user != null)
                {
                    this.user = user;

                    // Load the current user name.
                    var userNames = await jinagaClient.Query(Given<User>.Match((user, facts) =>
                        from name in facts.OfType<UserName>()
                        where name.user == user &&
                            !facts.Any<UserName>(next => next.prior.Contains(name))
                        select name
                    ), user);

                    // If the name is different, then update it.
                    if (userNames.Count != 1 || userNames.Single().value != profile.DisplayName)
                    {
                        await jinagaClient.Fact(new UserName(user, profile.DisplayName, userNames.ToArray()));
                    }
                }
            }
            return user;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
