using System.Diagnostics;
using System.Text.Json.Serialization;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("[{NodeType,nq}:{MessageId,nq}]")]
public class CqMessageReplyIdNode : CqMessageEntityNode, IMessageReplyIdNode
{
    [Ignore]
    public override MessageNodeType NodeType { get => (MessageNodeType)CqCodeType.Reply; }

    [Name("id")]
    public string MessageIdStr { get => MessageId.ToString(); set => MessageId = int.Parse(value); }

    [Ignore]
    public Int32 MessageId { get; set; }

    public CqMessageReplyIdNode(int messageId)
    {
        MessageId = messageId;
    }

    [JsonConstructor]
    public CqMessageReplyIdNode(string messageIdStr) : this(int.Parse(messageIdStr))
    { }

    public override string CqStringify()
    {
        return $"[CQ:reply,id={MessageIdStr}]";
    }
}