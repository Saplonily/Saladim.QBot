using System.Diagnostics;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("text: {Text}")]
public class CqMessageTextNode : CqMessageChainNodeModel
{
    [Ignore]
    public override MessageNodeType NodeType { get => MessageNodeType.Text; }

    [Name("text")]
    public string Text { get; set; } = default!;

    public CqMessageTextNode(string text)
        => Text = text;
}