using System.Collections;

namespace QBotDotnet.Core;

/// <summary>
/// 消息实体，表现为消息链
/// </summary>
public interface IMessageEntity : IEnumerable, IEnumerable<IMessageEntityNode>
{
}