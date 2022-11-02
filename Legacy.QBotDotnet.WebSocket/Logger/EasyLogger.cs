namespace QBotDotnet.GoCqHttp;

public static class Logger
{
    public static event Action<string>? OnLog;

    internal static void LogRaw(string head, string msg)
    {
        OnLog?.Invoke($"[{DateTime.Now.TimeOfDay} / {head}] {msg}");
    }

    public static void LogDetail(string msg)
    {
        LogRaw("Detail", msg);
    }
    public static void LogInfo(string msg)
    {
        LogRaw("Info", msg);
    }

    public static void LogException(string msg)
    {
        LogRaw("Exception", msg);
    }
}