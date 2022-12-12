using SaladimQBot.Core;
using SaladimQBot.Shared;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

public class FriendUser : User, IFriendUser, ICqMessageWindow
{
    protected FriendUser(CqClient client, long userId) : base(client, userId)
    {
    }

    internal new static FriendUser CreateFromMessagePost(in CqClient client, in CqMessagePost post)
    => new FriendUser(client, post.UserId)
            .LoadApiCallResult(post.UserId)
            .LoadFromUserId()
            .LoadFromMessageSender(post.Sender)
            .Cast<FriendUser>();

    internal new static FriendUser CreateFromNicknameAndId(in CqClient client, in string nickname, long userId)
        => new FriendUser(client, userId)
                .LoadApiCallResult(userId)
                .LoadFromUserId()
                .LoadNickname(nickname)
                .Cast<FriendUser>();

    internal new static FriendUser CreateFromId(in CqClient client, long userId)
        => new FriendUser(client, userId)
                .LoadApiCallResult(userId)
                .LoadFromUserId()
                .Cast<FriendUser>();

    async Task<IMessage> IMessageWindow.SendMessageAsync(IMessageEntity messageEntity)
        => await Client.SendFriendMessageAsync(UserId, new MessageEntity(Client, messageEntity));

    async Task<IMessage> IMessageWindow.SendMessageAsync(string rawString)
        => await Client.SendFriendMessageAsync(UserId, rawString);

    async Task<Message> ICqMessageWindow.SendMessageAsync(MessageEntity messageEntity)
        => await SendMessageAsync(messageEntity);

    async Task<Message> ICqMessageWindow.SendMessageAsync(string rawString)
        => await SendMessageAsync(rawString);

    public Task<FriendMessage> SendMessageAsync(MessageEntity messageEntity)
        => Client.SendFriendMessageAsync(UserId, messageEntity);

    public Task<FriendMessage> SendMessageAsync(string rawString)
        => Client.SendFriendMessageAsync(UserId, rawString);
}
