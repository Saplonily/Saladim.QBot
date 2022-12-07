namespace SaladimQBot.Core;

/// <summary>
/// <para>未知/未实现的消息节点</para>
/// <para>如果你发现了常见的节点没有被实现请随意提出issue</para>
/// </summary>
public interface IMessageChainUnimplementedNode : IMessageChainNode
{
    IDictionary<string, string> Params { get; }

    string NodeName { get; }
}