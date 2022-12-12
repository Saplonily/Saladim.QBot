using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class FriendMessage : PrivateMessage, IFriendMessage
{
    public new FriendUser Sender => ExpFriendSender.Value;

    public new FriendUser Author => Sender;

    public Expirable<FriendUser> ExpFriendSender { get; protected set; } = default!;

    public override ICqMessageWindow MessageWindow => Sender;

    protected internal FriendMessage(CqClient client, int messageId) : base(client, messageId)
    {
        TempSource = MessageTempSource.Invalid;
    }

    internal new static FriendMessage CreateFromPrivateMessagePost(CqClient client, CqPrivateMessagePost post)
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
            ).WithNoExpirable();
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

    public async new Task<FriendMessage> ReplyAsync(MessageEntity msg)
    {
        msg.Chain.MessageChainNodes.Insert(0, new MessageChainReplyNode(Client, this));
        var sentMessage = await this.Author.SendMessageAsync(msg);
        return sentMessage;
    }

    public async new Task<FriendMessage> ReplyAsync(string rawString)
    {
        var newString = ((new MessageChainReplyNode(Client, this)).ToModel().CqStringify()) + rawString;
        var sentMessage = await this.Author.SendMessageAsync(newString);
        return sentMessage;
    }
}
