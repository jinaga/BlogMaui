﻿using BlogMaui.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BlogMaui.Blog;
public partial class PostListViewModel : ObservableObject
{
    private readonly JinagaClient jinagaClient;
    private readonly UserProvider userProvider;
    private readonly ILogger<PostListViewModel> logger;

    private IObserver? observer;

    [ObservableProperty]
    private bool loading = true;

    [ObservableProperty]
    private string status = string.Empty;

    public ObservableCollection<PostHeaderViewModel> Posts { get; } = new();

    public ICommand Refresh { get; }

    public PostListViewModel(JinagaClient jinagaClient, UserProvider userProvider, ILogger<PostListViewModel> logger)
    {
        this.jinagaClient = jinagaClient;
        this.userProvider = userProvider;
        this.logger = logger;

        Refresh = new AsyncRelayCommand(HandleRefresh);
    }

    public async void Load(string domain)
    {
        try
        {
            observer?.Stop();
            observer = null;
            Posts.Clear();

            jinagaClient.OnStatusChanged += JinagaClient_OnStatusChanged;

            var postsInBlog = Given<Site>.Match((site, facts) =>
                from post in facts.OfType<Post>()
                where post.site == site &&
                    !facts.Any<PostDeleted>(deleted => deleted.post == post)
                select new
                {
                    post,
                    titles = facts.Observable(
                        from title in facts.OfType<PostTitle>()
                        where title.post == post &&
                            !facts.Any<PostTitle>(next => next.prior.Contains(title))
                        select title.value
                    )
                }
            );

            var user = await userProvider.GetUser();
            if (user == null)
            {
                return;
            }

            var site = new Site(user, domain);
            observer = jinagaClient.Watch(postsInBlog, site, projection =>
            {
                logger.LogInformation("Added post");
                var postHeaderViewModel = new PostHeaderViewModel(projection.post);
                projection.titles.OnAdded(title =>
                {
                    logger.LogInformation($"Updating title: {title}");
                    postHeaderViewModel.Title = title;
                });
                Posts.Add(postHeaderViewModel);

                return () =>
                {
                    Posts.Remove(postHeaderViewModel);
                };
            });

            Monitor(observer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while loading PostListViewModel");
        }
    }

    private async Task HandleRefresh()
    {
        if (observer == null)
        {
            return;
        }

        try
        {
            logger.LogInformation("Refreshing post list");
            Loading = true;
            await Task.WhenAll(
                observer.Refresh(),
                jinagaClient.Push());
            logger.LogInformation("Successfully refreshed post list");
        }
        catch (Exception x)
        {
            logger.LogError(x, "Error while refreshing");
        }
        finally
        {
            Loading = false;
        }
    }

    private async void Monitor(IObserver observer)
    {
        try
        {
            logger.LogInformation("Loading post list");
            bool wasInCache = await observer.Cached;
            if (!wasInCache)
            {
                await Task.WhenAll(
                    observer.Loaded,
                    jinagaClient.Push());
            }
            logger.LogInformation("Successfully loaded post list");
        }
        catch (Exception x)
        {
            logger.LogError(x, "Error while loading");
        }
        finally
        {
            Loading = false;
        }
    }

    public void Unload()
    {
        jinagaClient.OnStatusChanged -= JinagaClient_OnStatusChanged;

        observer?.Stop();
        observer = null;
        Posts.Clear();
    }

    private void JinagaClient_OnStatusChanged(JinagaStatus status)
    {
        if ((!status.IsSaving || status.LastSaveError != null) && status.QueueLength > 0)
        {
            // There are facts in the queue, and
            // the client is not saving, or has
            // experienced an error.
            Status = "Red";
        }
        else if (status.LastLoadError != null)
        {
            // There was an error last time the client
            // tried loading.
            Status = "Yellow";
        }
        else
        {
            Status = "Green";
        }
    }
}
