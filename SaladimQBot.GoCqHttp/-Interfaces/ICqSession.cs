namespace SaladimQBot.GoCqHttp;

public interface ICqSession : IDisposable
{
    bool Started { get; }

    Task StartAsync();

    /// <summary>
    /// 调用一个api
    /// </summary>
    /// <param name="api">要调用的api实体</param>
    /// <returns>
    /// 调用结果和状态码
    /// 10 - 正常,
    /// 20 - json文档解析出错,
    /// 21 - 调用成功但动作失败了,
    /// 22 - 调用超时,
    /// 23 - json实体解析出错,
    /// </returns>
    Task<(CqApiCallResult? result, int statusCode)> CallApiAsync(CqApi api);

    event OnCqSessionReceivedHandler OnReceived;

    event Action<Exception> OnErrorOccurred;
}