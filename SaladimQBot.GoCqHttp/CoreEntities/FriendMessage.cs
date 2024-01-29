using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

/// <summary>
/// 一个好友消息实体
/// </summary>
public class FriendMessage : PrivateMessage, IFriendMessage
{
    /// <summary>
    /// 消息发送者
    /// </summary>
    public new FriendUser Sender => ExpFriendSender.Value;

    /// <summary>
    /// 消息发送者
    /// </summary>
    public new FriendUser Author => Sender;

    /// <summary>
    /// 消息发送者 (可过期类型)
    /// </summary>
    public IExpirable<FriendUser> ExpFriendSender { get; protected set; } = default!;

    /// <summary>
    /// 消息窗口
    /// </summary>
    public override ICqMessageWindow MessageWindow => Sender;

    protected internal FriendMessage(CqClient client, int messageId) : base(client, messageId)
    {
        TempSource = MessageTempSource.Invalid;
    }

    internal static new FriendMessage CreateFromPrivateMessagePost(CqClient client, CqPrivateMessagePost post)
        => new FriendMessage(client, post.MessageId)
                .LoadFromPrivateMessagePost(post)
                .Cast<FriendMessage>();

    internal static new FriendMessage CreateFromMessageId(CqClient client, int messageId)
        => new FriendMessage(client, messageId)
                .LoadGetMessageApiResult().Cast<FriendMessage>()
                .LoadFromMessageId();

    internal new FriendMessage LoadFromMessageId()
    {
        base.LoadFromMessageId();
        ExpFriendSender = Client.MakeDependencyExpirable(
            ApiCallResult,
            d => FriendUser.CreateFromNicknameAndId(Client, d.Sender.Nickname, d.Sender.UserId)
            );
        return this;
    }

    internal new FriendMessage LoadFromPrivateMessagePost(CqPrivateMessagePost post)
    {
        base.LoadFromPrivateMessagePost(post);
        ExpFriendSender = Client.MakeNoneExpirableExpirable(
            FriendUser.CreateFromNicknameAndId(Client, post.Sender.Nickname, post.Sender.UserId)
            );
        return this;

    }
}
