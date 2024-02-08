﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jinaga;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BlogMaui.Areas.Blog.Posts;
public partial class PostListViewModel : ObservableObject, IQueryAttributable
{
    private readonly JinagaClient jinagaClient;
    private readonly ILogger<PostListViewModel> logger;

    private IObserver? observer;

    [ObservableProperty]
    private bool loading = false;

    public ObservableCollection<PostHeaderViewModel> Posts { get; } = new();

    public ICommand Refresh { get; }

    public PostListViewModel(JinagaClient jinagaClient, ILogger<PostListViewModel> logger)
    {
        this.jinagaClient = jinagaClient;
        this.logger = logger;

        Refresh = new AsyncRelayCommand(HandleRefresh);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Loading = true;

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

        logger.LogInformation("Getting site");
        var site = query.GetParameter<Site>("site");
        logger.LogInformation($"Got site: {site.domain}");
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

    public void Unload()
    {
        observer?.Stop();
        observer = null;
        Posts.Clear();
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
}