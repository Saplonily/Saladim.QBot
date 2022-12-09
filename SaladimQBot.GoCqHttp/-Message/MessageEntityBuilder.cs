namespace SaladimQBot.GoCqHttp;

public class MessageEntityBuilder
{
    protected CqClient client;
    protected MessageChain chain;

    public MessageEntityBuilder(CqClient client)
    {
        this.client = client;
        chain = new(client);
    }

    public MessageEntityBuilder WithText(string text)
    {
        chain.MessageChainNodes.Add(new MessageChainTextNode(client, text));
        return this;
    }

    public MessageEntityBuilder WithTextLine(string text) => this.WithText(text + "\r\n");

    public MessageEntityBuilder WithImage(string uri)
    {
        chain.MessageChainNodes.Add(new MessageChainImageNode(client, uri));
        return this;
    }

    public MessageEntityBuilder WithAt(long userId)
    {
        chain.MessageChainNodes.Add(new MessageChainAtNode(client, userId));
        return this;
    }

    public MessageEntityBuilder WithAt(long userId, string nameWhenUserNotExists)
    {
        chain.MessageChainNodes.Add(new MessageChainAtNode(client, userId, nameWhenUserNotExists));
        return this;
    }

    public MessageEntityBuilder WithAt(User user)
    {
        chain.MessageChainNodes.Add(new MessageChainAtNode(client, user.UserId));
        return this;
    }

    public MessageEntityBuilder WithAt(User user, string nameWhenUserNotExists)
    {
        chain.MessageChainNodes.Add(new MessageChainAtNode(client, user.UserId, nameWhenUserNotExists));
        return this;
    }

    public MessageEntityBuilder WithFace(int faceId)
    {
        chain.MessageChainNodes.Add(new MessageChainFaceNode(client, faceId));
        return this;
    }

    public MessageEntityBuilder WithReply(Message message)
    {
        chain.MessageChainNodes.Add(new MessageChainReplyNode(client, message));
        return this;
    }

    public MessageEntity Build(bool prepareRawString = false)
    {
        if (prepareRawString)
        {
            var entity = new MessageEntity(client, chain);
            _ = entity.RawString;
            return entity;
        }
        else
        {
            return new MessageEntity(client, chain);
        }
    }
}
