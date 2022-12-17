using System;
using SaladimQBot.Core;

namespace SaladimQBot.MessagePipeline;

public class MessagePipeline
{
    public delegate Task Middleware(IMessage message, Middleware next);
    protected List<Middleware> middlewares;

    public MessagePipeline()
    {
        middlewares = new();
    }

    public void InsertMiddlewareToFront(Middleware middleware)
    {
        middlewares.Insert(0, middleware);
    }

    public void AppendMiddleware(Middleware middleware)
    {
        middlewares.Add(middleware);
    }

    public void Execute(IMessage msg)
    {
        var count = middlewares.Count;
        for (int i = 0; i < count; i++)
        {
            Middleware next;
            if (i + 1 == count)
            {
                next = (_, _) => Task.CompletedTask;
            }
            else
            {
                next = middlewares[i + 1];
            }
            middlewares[i].Invoke(msg, next);
        }
    }
}
