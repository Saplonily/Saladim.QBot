using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Message : CqEntity, IMessage
{
    public Expirable<MessageEntity> ExpMessageEntity { get; protected set; } = default!;

    public Expirable<User> ExpSender { get; protected set; } = default!;

    public MessageEntity MessageEntity => ExpMessageEntity.Value;

    public virtual ICqMessageWindow MessageWindow => Sender;

    public User Sender => ExpSender.Value;

    public User Author => Sender;

    public int MessageId { get; protected set; } = default!;

    protected internal Expirable<GetMessageActionResultData> ApiCallResult { get; set; } = default!;

    protected internal Message(CqClient client, int messageId)
        : base(client)
    {
        MessageId = messageId;
    }

    public Task RecallAsync()
        => Client.RecallMessageAsync(this.MessageId);

    #region loadÔÓÆßÔÓ°ËµÄ

    internal static Message CreateFromMessagePost(CqClient client, CqMessagePost post)
        => new Message(client, post.MessageId)
            .LoadFromMessagePost(post);

    internal static Message CreateFromMessageId(CqClient client, int messageId)
        => new Message(client, messageId)
            .LoadGetMessageApiResult()
            .LoadFromMessageId();

    internal Message LoadFromMessagePost(CqMessagePost post)
    {
        ExpSender = Client.MakeNoneExpirableExpirable(User.CreateFromMessagePost(Client, post));
        ExpMessageEntity = Client.MakeNoneExpirableExpirable(new MessageEntity(post.MessageEntity, post.RawMessage));
        return this;
    }

    internal Message LoadFromMessageId()
    {
        ExpSender = Client.MakeDependencyExpirable(
            ApiCallResult,
            d => User.CreateFromNicknameAndId(Client, d.Sender.Nickname, d.Sender.UserId)
            ).WithNoExpirable();
        ExpMessageEntity = Client.MakeDependencyExpirable(
            ApiCallResult,
            d => new MessageEntity(d.MessageEntity, MessageChainHelper.ChainToRawString(d.MessageEntity))
            ).WithNoExpirable();
        return this;
    }

    internal Message LoadGetMessageApiResult()
    {
        GetMessageAction api = new()
        {
            MessageId = this.MessageId
        };
        ApiCallResult = this.Client.MakeExpirableApiCallResultData<GetMessageActionResultData>(api);
        return this;
    }

    #endregion

    #region IMessage

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IMessageEntity IMessage.MessageEntity => MessageEntity;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IUser IMessage.Sender => Sender;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IMessageWindow IMessage.MessageWindow => Sender;

    #endregion

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{Sender.Nickname}({Sender.UserId}): {MessageEntity.RawString}";

    public override bool Equals(object? obj)
    {
        return obj is Message message &&
               this.MessageId == message.MessageId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.MessageId);
    }

    public static bool operator ==(Message? left, Message? right)
    {
        return EqualityComparer<Message>.Default.Equals(left, right);
    }

    public static bool operator !=(Message? left, Message? right)
    {
        return !(left == right);
    }
}