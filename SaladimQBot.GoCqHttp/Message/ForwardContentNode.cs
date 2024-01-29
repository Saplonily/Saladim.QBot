using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class ForwardContentNode : ForwardNode, IForwardContentNode
{
    public string SenderShowName { get; protected set; }

    public User Sender { get; protected set; }

    public MessageEntity MessageEntity { get; protected set; }

    public DateTime SendTime { get; protected set; }

    public ForwardContentNode(
        string senderShowName,
        User sender,
        MessageEntity messageEntity,
        DateTime sendTime
        )
    {
        this.SenderShowName = senderShowName;
        this.Sender = sender;
        this.MessageEntity = messageEntity;
        this.SendTime = sendTime;
    }

    IUser IForwardContentNode.Sender => Sender;

    IMessageEntity IForwardContentNode.MessageEntity => MessageEntity;

}
