namespace SaladimQBot.Core;

/// <summary>
/// <para>消息链中的一个节点，可以表现为文本节点、图像节点、回复节点、表情节点等</para>
/// </summary>
public interface IMessageChainNode
{
    MessageNodeType NodeType { get; }
}