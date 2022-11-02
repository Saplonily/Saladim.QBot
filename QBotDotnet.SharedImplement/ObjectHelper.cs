using System.Diagnostics;

namespace QBotDotnet.SharedImplement;

//奇奇怪怪的helper
public static class ObjectHelper
{
    [DebuggerStepThrough]
    public static void BulkRun<T>(Action<T> action, params T[] objs)
    {
        foreach (var obj in objs)
        {
            action.Invoke(obj);
        }
    }

    [DebuggerStepThrough]
    public static async Task BulkRunAsync<T>(Func<T, Task> action, params T[] objs)
    {
        foreach (var obj in objs)
        {
            await action.Invoke(obj);
        }
    }

    public static async Task BulkRunAsync<T>(Action<T> action, params T[] objs)
    {
        await Task.Run(() =>
        {
            foreach (var obj in objs)
            {
                action.Invoke(obj);
            }
        });

    }
}