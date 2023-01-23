namespace SaladimQBot.Extensions;

public class Pipeline<T> where T : class
{
    public delegate Task Middleware(T thing, Func<Task> next);
    protected List<Middleware> middlewares;

    public Pipeline()
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

    public Task ExecuteAsync(T thing)
        => ExecuteAtAsync(0, thing);

    protected async Task ExecuteAtAsync(int index, T thing)
    {
        //怎么会有人创建管道不加中间件就执行的啊
        if (middlewares.Count == 0) return;
        var middleware = middlewares[index];
        Func<Task> next;
        if (index + 1 == middlewares.Count)
            next = () => Task.CompletedTask;
        else
            next = () => ExecuteAtAsync(index + 1, thing);
        await middlewares[index].Invoke(thing, next).ConfigureAwait(false);
    }
}
