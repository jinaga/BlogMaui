﻿using BlogMaui.Blog;
using Jinaga;
using System.Text.Json;

namespace BlogMaui.Authentication;
public class UserProvider
{
    private const string PublicKeyKey = "BlogMaui.PublicKey";

    private readonly JinagaClient jinagaClient;
    private readonly SemaphoreSlim semaphore = new(1);
    private User? user;

    public UserProvider(JinagaClient jinagaClient)
    {
        this.jinagaClient = jinagaClient;
    }

    public async Task Initialize()
    {
        await LoadUser();
    }

    public async Task ClearUser()
    {
        await semaphore.WaitAsync();
        try
        {
            this.user = null;
            await SaveUser();
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
                    await SaveUser();

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
        catch (Exception ex)
        {
            Console.WriteLine($"Error while getting user: {ex.Message}");
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task LoadUser()
    {
        string? publicKey = await SecureStorage.GetAsync(PublicKeyKey);
        if (publicKey != null)
        {
            this.user = new User(publicKey);
        }
    }

    private async Task SaveUser()
    {
        if (user == null)
        {
            SecureStorage.Remove(PublicKeyKey);
        }
        else
        {
            await SecureStorage.SetAsync(PublicKeyKey, user.publicKey);
        }
    }
}