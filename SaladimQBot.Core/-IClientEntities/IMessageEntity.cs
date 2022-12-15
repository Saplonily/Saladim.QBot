namespace SaladimQBot.Core;

/// <summary>
/// 消息实体，表现为消息链
/// </summary>
public interface IMessageEntity : IClientEntity
{
    /// <summary>
    /// 该消息实体的消息链
    /// </summary>
    IMessageChain Chain { get; }

    /// <summary>
    /// 特殊遍码格式的字符串
    /// </summary>
    string RawString { get; }
}