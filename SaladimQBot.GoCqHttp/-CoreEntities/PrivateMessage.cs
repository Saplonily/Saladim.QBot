using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class PrivateMessage : Message, IPrivateMessage
{
    public MessageTempSource TempSource { get; protected set; } = default!;

    public long? TempSourceGroupId { get; protected set; } = null;

    public Expirable<User> ExpPrivateSender { get => ExpSender; }

    public User PrivateSender { get => ExpPrivateSender.Value; }

    public override ICqMessageWindow MessageWindow =>
        throw new InvalidOperationException("Raw PrivateMessage hasn't MessageWindow.");

    protected internal PrivateMessage(CqClient client, int messageId) : base(client, messageId)
    {
        IsFromGroup = client.MakeNoneExpirableExpirable(false);
    }

    /// <summary>
    /// 回复一个群消息,
    /// 注意回复过程中不要再次引用消息实体,
    /// 回复过程中会向消息链前端加入回复节点
    /// </summary>
    /// <param name="msg">使用的消息实体</param>
    /// <returns>发送的消息</returns>
    [Obsolete(
        "请勿使用此方法有可能的回复临时消息, 可能会导致账号冻结, " +
        "具体消息请见 SaladimQBot.GoCqHttp.User的方法 SendMessageAsync的xml注释, " +
        "如果您仅想回复好友信息请自行进行类型转换"
        )]
    public async Task<PrivateMessage> ReplyAsync(MessageEntity msg)
    {
        msg.Chain.MessageChainNodes.Insert(0, new MessageChainReplyNode(Client, this));
        var sentMessage = await this.Sender.SendMessageAsync(TempSourceGroupId, msg);
        msg.Chain.MessageChainNodes.RemoveAt(0);
        return sentMessage;
    }

    [Obsolete(
        "请勿使用此方法有可能的回复临时消息, 可能会导致账号冻结, " +
        "具体消息请见 SaladimQBot.GoCqHttp.User的方法 SendMessageAsync的xml注释, " +
        "如果您仅想回复好友信息请自行进行类型转换"
        )]
    public async Task<PrivateMessage> ReplyAsync(string rawString)
    {
        var newString = ((new MessageChainReplyNode(Client, this)).ToModel().CqStringify()) + rawString;
        var sentMessage = await this.Sender.SendMessageAsync(TempSourceGroupId, newString);
        return sentMessage;
    }

    #region load一大堆

    internal static PrivateMessage CreateFromPrivateMessagePost(CqClient client, CqPrivateMessagePost post)
        => new PrivateMessage(client, post.MessageId)
            .LoadFromPrivateMessagePost(post);

    internal static new PrivateMessage CreateFromMessageId(CqClient client, int messageId)
        => new PrivateMessage(client, messageId)
                .LoadGetMessageApiResult()
                .LoadFromMessageId().Cast<PrivateMessage>()
                .LoadMessageTempSourceFromId();

    internal PrivateMessage LoadFromPrivateMessagePost(CqPrivateMessagePost post)
    {
        base.LoadFromMessagePost(post);
        TempSource = post.TempSource;
        if (post.Sender is CqGroupTempMessageSender ts)
        {
            TempSourceGroupId = ts.GroupId;
        }
        return this;
    }

    internal PrivateMessage LoadMessageTempSourceFromId()
    {
        TempSource = MessageTempSource.Invalid;
        return this;
    }

    #endregion

    #region IPrivateMessage

    Core.MessageTempSource IPrivateMessage.TempSource => TempSource.Cast<Core.MessageTempSource>();

    #endregion
}


