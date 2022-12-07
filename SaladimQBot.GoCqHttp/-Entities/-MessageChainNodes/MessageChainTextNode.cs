using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{Text}")]
public class MessageChainTextNode : MessageChainNode, IMessageChainTextNode
{
    public override CqCodeType NodeType => CqCodeType.Text;

    MessageNodeType IMessageChainNode.NodeType => NodeType.Cast<MessageNodeType>();

    public string Text { get; set; }

    internal const string TextProperty = "text";

    public MessageChainTextNode(CqClient client, string text) : base(client)
    {
        Text = text;
    }

    internal override CqMessageChainNodeModel ToModel()
    {
        StringDictionary dic = new()
        {
            ["text"] = this.Text
        };
        return new(NodeType, dic);
    }
}
