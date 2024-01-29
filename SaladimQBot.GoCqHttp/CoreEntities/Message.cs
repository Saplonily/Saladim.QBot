using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

/// <summary>
/// 一个消息实体
/// </summary>
[DebuggerDisplay("{Sender.Nickname}({Sender.UserId}): {MessageEntity.RawString}")]
public class Message : CqEntity, IMessage
{
    public IExpirable<MessageEntity> ExpMessageEntity { get; protected set; } = default!;

    public IExpirable<User> ExpSender { get; protected set; } = default!;

    public IExpirable<bool>? IsFromGroup { get; protected set; } = null;

    public IExpirable<DateTime> SendTime { get; protected set; } = default!;

    public MessageEntity MessageEntity => ExpMessageEntity.Value;

    /// <summary>
    /// 消息窗口
    /// </summary>
    public virtual ICqMessageWindow MessageWindow =>
        throw new InvalidOperationException("Raw Message hasn't MessageWindow.");

    IMessageWindow IMessage.MessageWindow => MessageWindow;

    /// <summary>
    /// 消息发送者
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public User Sender => ExpSender.Value;

    /// <summary>
    /// 消息发送者
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public User Author => Sender;

    /// <summary>
    /// 消息id
    /// </summary>
    public int MessageId { get; protected set; } = default!;

    protected internal IDependencyExpirable<GetMessageActionResultData> ApiCallResult { get; set; } = default!;

    protected internal Message(CqClient client, int messageId)
        : base(client)
    {
        MessageId = messageId;
    }

    /// <summary>
    /// 撤回该条消息
    /// </summary>
    /// <returns></returns>
    public Task RecallAsync()
        => Client.RecallMessageAsync(this.MessageId);

    #region load杂七杂八的

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
        ExpMessageEntity = Client.MakeNoneExpirableExpirable(new MessageEntity(Client, post.MessageChainModel, post.RawMessage));
        SendTime = Client.MakeNoneExpirableExpirable(DateTimeHelper.GetFromUnix(post.Time));
        return this;
    }

    internal Message LoadFromMessageId()
    {
        ExpSender = Client.MakeDependencyExpirable(ApiCallResult, d => User.CreateFromNicknameAndId(Client, d.Sender.Nickname, d.Sender.UserId));
        IsFromGroup = Client.MakeDependencyExpirable(ApiCallResult, d => d.IsGroupMessage);
        SendTime = Client.MakeDependencyExpirable(ApiCallResult, d => DateTimeHelper.GetFromUnix(d.Time));
        ExpMessageEntity = Client.MakeDependencyExpirable(
            ApiCallResult,
            d => new MessageEntity(Client, d.MessageEntity, MessageChainModelHelper.ChainToRawString(d.MessageEntity))
            );
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
    DateTime IMessage.SendTime => SendTime.Value;


    #endregion

    #region 重写及杂七杂八的
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
        return EqualityComparer<Message>.Default.Equals(left!, right!);
    }

    public static bool operator !=(Message? left, Message? right)
    {
        return !(left == right);
    }

    #endregion
}