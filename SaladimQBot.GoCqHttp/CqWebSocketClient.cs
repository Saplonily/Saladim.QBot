using Saladim.SalLogger;

namespace SaladimQBot.GoCqHttp;

public sealed class CqWebSocketClient : CqClient
{
    private readonly TimeSpan expireTime = new(0, 3, 14);

    public override TimeSpan ExpireTimeSpan => expireTime;

    /// <summary>
    /// 使用gocqHttp的ws地址初始化一个WebSocket客户端
    /// </summary>
    /// <param name="gocqHttpAddress">ws地址,例如<code>ws://127.0.0.1:5000</code></param>
    /// <param name="logLevelLimit">日志等级限制, 低于该等级的日志将不会触发<see cref="CqClient.OnLog"/>事件</param>
    /// <param name="authorization">authorization值</param>
    public CqWebSocketClient(string gocqHttpAddress, LogLevel logLevelLimit = LogLevel.Info, string? authorization = null)
        : base(
            logLevelLimit,
            new CqWebSocketSession(gocqHttpAddress, "api", useApiEndPoint: true, authorization: authorization),
            new CqWebSocketSession(gocqHttpAddress, "event", useEventEndPoint: true, authorization: authorization)
              )
    {
    }
}