using System.Text;
using SaladimQBot.Core;

namespace SaladimQBot.Core;

/// <summary>
/// 具有实体类的<see cref="UniNode"/>基类
/// </summary>
public abstract class TypedUniNode : UniNode
{
    public abstract MessageNodeType Type { get; }

    public override string Name => Type switch
    {
        MessageNodeType.Text => "text",
        MessageNodeType.Unimplemented => throw new InvalidOperationException("Try to get unimpl node in a none-UnimplNode."),
        MessageNodeType.Invalid => throw new InvalidOperationException("Invalid node type."),
        _ => UniNodeMapper.NodeTypeToDisplayName(Type) ?? throw new NotSupportedException("Unknown node type.")
    };

    public TypedUniNode(IClient client) : base(client)
    {
    }

    public override string ToFormattedText()
        => UniNode.ToFormattedText(Name, null, Deconstruct());
}
