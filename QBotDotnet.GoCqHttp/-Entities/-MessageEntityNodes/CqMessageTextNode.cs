using System.Diagnostics;
using System.Text.Json.Serialization;
using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;

[DebuggerDisplay("text: {Text}")]
public class CqMessageTextNode : CqMessageEntityNode, IMessageTextNode
{
    [Ignore]
    public override MessageNodeType NodeType { get => MessageNodeType.Text; }

    [Name("text")]
    public string Text { get; set; } = default!;

    public CqMessageTextNode(string text)
        => Text = text;
}