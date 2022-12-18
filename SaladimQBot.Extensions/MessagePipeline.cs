using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public class MessagePipeline
{
    public delegate Task Middleware(IMessage message, Func<Task> next);
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

    public Task ExecuteAsync(IMessage msg)
        => ExecuteAtAsync(0, msg);

    protected async Task ExecuteAtAsync(int index, IMessage msg)
    {
        var middleware = middlewares[index];
        Func<Task> next;
        if (index + 1 == middlewares.Count)
        {
            next = () => Task.CompletedTask;
        }
        else
        {
            next = () => ExecuteAtAsync(index + 1, msg);
        }
        await middlewares[index].Invoke(msg, next).ConfigureAwait(false);
    }
}
