using Saladim.SalLogger;

namespace SaladimQBot.GoCqHttp;

public sealed class CqHttpClient : CqClient
{
    private readonly ICqSession apiSession;
    private readonly ICqSession postSession;
    private readonly TimeSpan expireTimeSpan = new(0, 3, 14);

    public override ICqSession ApiSession => apiSession;

    public override ICqSession PostSession => postSession;

    public override TimeSpan ExpireTimeSpan => expireTimeSpan;

    public CqHttpClient(string requestUrl, string listenerUrl, LogLevel logLevelLimit) : base(logLevelLimit)
    {
        apiSession = new CqHttpRequestorSession(requestUrl);
        postSession = new CqHttpListenerSession(listenerUrl);
    }
}
