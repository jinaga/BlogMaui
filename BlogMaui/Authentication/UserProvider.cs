﻿using System;
using System.Collections.Immutable;
using Jinaga;

namespace BlogMaui.Authentication;

public class UserProvider
{
    private readonly object syncRoot = new object();
    private User? user;
    private ImmutableList<Handler> handlers = ImmutableList<Handler>.Empty;

    public class Handler
    {
        public Func<User, Action> WithUser { get; }
        public Action Clear { get; set; } = () => { };

        public Handler(Func<User, Action> withUser)
        {
            WithUser = withUser;
        }
    }

    public void SetUser(User user)
    {
        lock (syncRoot)
        {
            BeforeSetUser();
            this.user = user;
            AfterSetUser();
        }
    }

    public void ClearUser()
    {
        lock (syncRoot)
        {
            BeforeSetUser();
            user = null;
            AfterSetUser();
        }
    }

    public Handler AddHandler(Func<User, Action> withUser)
    {
        lock (syncRoot)
        {
            var handler = new Handler(withUser);
            handlers = handlers.Add(handler);
            if (user != null)
            {
                handler.Clear = handler.WithUser(user);
            }
            return handler;
        }
    }

    public void RemoveHandler(Handler handler)
    {
        lock (syncRoot)
        {
            handlers = handlers.Remove(handler);
            if (user != null)
            {
                handler.Clear();
            }
        }
    }

    private void BeforeSetUser()
    {
        // Assumes this is called within a lock
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
        // Assumes this is called within a lock
        if (user != null)
        {
            foreach (var handler in handlers)
            {
                handler.Clear = handler.WithUser(user);
            }
        }
    }
}