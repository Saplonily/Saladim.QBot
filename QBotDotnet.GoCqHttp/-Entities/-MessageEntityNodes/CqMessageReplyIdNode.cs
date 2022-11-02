using System.Diagnostics;
using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;

[DebuggerDisplay("[{NodeType,nq}:{MessageId,nq}]")]
public class CqMessageReplyIdNode : CqMessageEntityNode, IMessageReplyIdNode
{
    [Ignore]
    public override MessageNodeType NodeType { get => (MessageNodeType)CqCodeType.Reply; }

    [Name("id")]
    public string MessageIdStr { get => MessageId.ToString(); set => MessageId = int.Parse(value); }

    public Int32 MessageId { get; set; }
}