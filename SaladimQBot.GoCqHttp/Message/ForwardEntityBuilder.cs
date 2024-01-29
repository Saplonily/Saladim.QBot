using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class ForwardEntityBuilder : CqEntity, IForwardEntityBuilder
{
    private List<ForwardNode> nodes;

    public ForwardEntityBuilder(CqClient client) : base(client)
    {
        nodes = new();
    }

    public ForwardEntityBuilder AddMessage(Message msg)
    {
        if (msg is GroupMessage groupMsg)
            nodes.Add(
                new ForwardContentNode(
                    groupMsg.Author.CardOrNickname,
                    groupMsg.Author,
                    msg.MessageEntity,
                    msg.SendTime.Value
                    )
                );
        else
            nodes.Add(new ForwardContentNode(msg.Author.Nickname.Value, msg.Author, msg.MessageEntity, msg.SendTime.Value));
        return this;
    }

    public ForwardEntityBuilder AddMessage(string senderShowName, User sender, MessageEntity entity, DateTime sendTime)
    {
        nodes.Add(new ForwardContentNode(senderShowName, sender, entity, sendTime));
        return this;
    }

    public ForwardEntityBuilder AddMessage(User sender, MessageEntity entity, DateTime sendTime)
    {
        if (sender is GroupUser groupUser)
        {
            this.AddMessage(groupUser.CardOrNickname, sender, entity, sendTime);
        }
        else
        {
            this.AddMessage(sender.Nickname.Value, sender, entity, sendTime);
        }
        return this;
    }

    public ForwardEntity Build()
    {
        return new(Client, nodes);
    }

    IForwardEntityBuilder IForwardEntityBuilder.AddMessage(IMessage msg)
        => msg is Message ourMsg ?
            AddMessage(ourMsg) :
            throw new InvalidOperationException(StringConsts.NotSameClientError);

    IForwardEntityBuilder IForwardEntityBuilder.AddMessage(string senderShowName, IUser sender, IMessageEntity entity, DateTime sendTime)
        => sender is User ourSender && entity is MessageEntity ourEntity ?
            AddMessage(senderShowName, ourSender, ourEntity, sendTime) :
            throw new InvalidOperationException(StringConsts.NotSameClientError);

    IForwardEntityBuilder IForwardEntityBuilder.AddMessage(IUser sender, IMessageEntity entity, DateTime sendTime)
        => sender is User ourSender && entity is MessageEntity ourEntity ?
            AddMessage(ourSender, ourEntity, sendTime) :
            throw new InvalidOperationException(StringConsts.NotSameClientError);

    IForwardEntity IForwardEntityBuilder.Build()
        => this.Build();
}
