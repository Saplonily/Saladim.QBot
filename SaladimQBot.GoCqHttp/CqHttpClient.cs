using Saladim.SalLogger;

namespace SaladimQBot.GoCqHttp;

public sealed class CqHttpClient : CqClient
{
    private readonly TimeSpan expireTimeSpan = new(0, 3, 14);

    public override TimeSpan ExpireTimeSpan => expireTimeSpan;

    public CqHttpClient(string requestUrl, string listenerUrl, string authorization, LogLevel logLevelLimit) :
        base(logLevelLimit, new CqHttpRequestorSession(requestUrl, authorization), new CqHttpListenerSession(listenerUrl))
    {

    }
}
