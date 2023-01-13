using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class MessageEntityBuilder : CqEntity, IMessageEntityBuilder
{
    protected MessageChain chain;

    public MessageEntityBuilder(CqClient client) : base(client)
    {
        chain = new(client);
    }

    public MessageEntityBuilder WithText(string text)
    {
        chain.MessageChainNodes.Add(new MessageChainTextNode(Client, text));
        return this;
    }

    public MessageEntityBuilder WithTextLine(string text) => this.WithText(text + "\r\n");

    public MessageEntityBuilder WithImage(Uri uri)
    {
        chain.MessageChainNodes.Add(new MessageChainImageNode(Client, uri));
        return this;
    }

    public MessageEntityBuilder WithAt(long userId)
    {
        chain.MessageChainNodes.Add(new MessageChainAtNode(Client, userId));
        return this;
    }

    public MessageEntityBuilder WithAt(long userId, string nameWhenUserNotExists)
    {
        chain.MessageChainNodes.Add(new MessageChainAtNode(Client, userId, nameWhenUserNotExists));
        return this;
    }

    public MessageEntityBuilder WithAt(User user)
    {
        chain.MessageChainNodes.Add(new MessageChainAtNode(Client, user.UserId));
        return this;
    }

    public MessageEntityBuilder WithAt(User user, string nameWhenUserNotExists)
    {
        chain.MessageChainNodes.Add(new MessageChainAtNode(Client, user.UserId, nameWhenUserNotExists));
        return this;
    }

    public MessageEntityBuilder WithFace(int faceId)
    {
        chain.MessageChainNodes.Add(new MessageChainFaceNode(Client, faceId));
        return this;
    }

    public MessageEntityBuilder WithReply(Message message)
    {
        chain.MessageChainNodes.Add(new MessageChainReplyNode(Client, message));
        return this;
    }

    public MessageEntityBuilder WithUnImpl(string name, IDictionary<string, string> args)
    {
        chain.MessageChainNodes.Add(new MessageChainUnimplementedNode(Client, name, args));
        return this;
    }

    public MessageEntity Build(bool prepareRawString = false)
    {
        if (prepareRawString)
        {
            var entity = new MessageEntity(Client, chain);
            _ = entity.RawString;
            return entity;
        }
        else
        {
            return new MessageEntity(Client, chain);
        }
    }

    IMessageEntityBuilder IMessageEntityBuilder.WithText(string text)
        => WithText(text);

    IMessageEntityBuilder IMessageEntityBuilder.WithImage(Uri uri)
        => WithImage(uri);

    IMessageEntityBuilder IMessageEntityBuilder.WithAt(long userId)
        => WithAt(userId);

    IMessageEntityBuilder IMessageEntityBuilder.WithAt(long userId, string nameWhenUserNotExists)
        => WithAt(userId, nameWhenUserNotExists);

    IMessageEntityBuilder IMessageEntityBuilder.WithFace(int faceId)
        => WithFace(faceId);

    IMessageEntityBuilder IMessageEntityBuilder.WithReply(IMessage message)
        => WithReply(message is Message ourMessage ? ourMessage : throw new InvalidOperationException("Only support message in the same client."));

    IMessageEntityBuilder IMessageEntityBuilder.WithUnImpl(string name, IDictionary<string, string> args)
        => WithUnImpl(name, args);

    IMessageEntity IMessageEntityBuilder.Build(bool prepareRawString)
        => Build();
}
