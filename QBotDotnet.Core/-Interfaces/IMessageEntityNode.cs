namespace QBotDotnet.Core;

/// <summary>
/// <para>消息链中的一个节点，可以表现为文本节点、图像节点、回复节点、表情节点等</para>
/// <para>截止 2022-10-20 暂未支持实体类消息节点</para>
/// <para>截止 2022-10-28 实体类消息节点已被支持</para>
/// </summary>
public interface IMessageEntityNode
{
    MessageNodeType NodeType { get; }
}