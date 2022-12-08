using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class MessageChainReplyNode : MessageChainNode, IMessageChainReplyNode
{
    public override CqCodeType NodeType => CqCodeType.Reply;

    public Message MessageBeReplied { get; set; }

    public MessageChainReplyNode(CqClient client, Message messageBeReplied) : base(client)
    {
        MessageBeReplied = messageBeReplied;
    }

    internal override CqMessageChainNodeModel ToModel()
    {
        StringDictionary dic = new()
        {
            ["id"] = MessageBeReplied.MessageId.ToString()
        };
        return new(NodeType, dic);
    }

    IMessage IMessageChainReplyNode.MessageBeReplied => MessageBeReplied;
}
