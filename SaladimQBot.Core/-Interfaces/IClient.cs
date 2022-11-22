namespace SaladimQBot.Core;

/// <summary>
/// <para>Client类需要实现的接口</para>
/// </summary>
public interface IClient
{
    IGroupMessage GetGroupMessageById(long messageId);

    /// <summary>
    /// 开始该Client与go-cqhttp的连接
    /// </summary>
    /// <returns>状态值</returns>
    Task StartAsync();

    /// <summary>
    /// 停止该Client与go-cqhttp的连接
    /// </summary>
    /// <returns>状态值</returns>
    Task StopAsync();
}