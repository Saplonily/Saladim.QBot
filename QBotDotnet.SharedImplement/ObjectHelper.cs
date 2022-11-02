namespace QBotDotnet.SharedImplement;

//奇奇怪怪的helper
public static class ObjectHelper
{
    public static void BulkRun<T>(Action<T> action, params T[] objs)
    {
        foreach (var obj in objs)
        {
            action.Invoke(obj);
        }
    }

    public static async Task BulkRunAsync<T>(Action<T> action, params T[] objs)
    {
        await Task.Run(() =>
        {
            BulkRun(action, objs);
        });
    }
}