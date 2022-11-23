using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Message : CqEntity, IMessage
{
    public Expirable<MessageEntity> ExpMessageEntity { get; protected set; } = default!;

    public Expirable<User> ExpSender { get; protected set; } = default!;

    public MessageEntity MessageEntity => ExpMessageEntity.Value;

    public User Sender => ExpSender.Value;

    public long MessageId { get; protected set; } = default!;

    protected internal Expirable<GetMessageActionResultData> ApiCallResult { get; set; } = default!;

    protected internal Message(ICqClient client, long messageId)
        : base(client)
    {
        MessageId = messageId;
    }

    internal static Message CreateFromMessagePost(ICqClient client, CqMessagePost post)
        => new Message(client, post.MessageId)
            .LoadFromMessagePost(post);

    internal static Message CreateFromMessageId(ICqClient client, long messageId)
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
            d => new MessageEntity(d.MessageEntity, MessageEntityHelper.CqEntity2RawString(d.MessageEntity))
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

    #region IMessage

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IMessageEntity IMessage.MessageEntity => MessageEntity;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IUser IMessage.Sender => Sender;

    #endregion

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{Sender.Nickname}({Sender.UserId}): {MessageEntity.RawString}";

}