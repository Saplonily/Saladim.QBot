using System.Collections;

namespace SaladimQBot.Core;

/// <summary>
/// 消息实体，表现为消息链
/// </summary>
public interface IMessageEntity : IReadOnlyCollection<IMessageEntityNode>
{
    /// <summary>
    /// cq码格式的字符串
    /// </summary>
    string RawString { get; }
}