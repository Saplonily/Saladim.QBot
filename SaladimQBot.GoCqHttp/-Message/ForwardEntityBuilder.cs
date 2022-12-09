namespace SaladimQBot.GoCqHttp;

public class ForwardEntityBuilder
{
    private readonly CqClient client;
    private List<ForwardNode> nodes;

    public ForwardEntityBuilder(CqClient client)
    {
        nodes = new();
        this.client = client;
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
        return new(client, nodes);
    }
}
