namespace SaladimQBot.Core;

public static class EntityExtensions
{
    #region 消息相关

    #region FriendUser私聊发送消息

    public static Task<IFriendMessage> SendMessageAsync(this IFriendUser user, IMessageEntity messageEntity)
        => user.Client.SendFriendMessageAsync(user.UserId, messageEntity);

    public static Task<IFriendMessage> SendMessageAsync(this IFriendUser user, string rawString)
        => user.Client.SendFriendMessageAsync(user.UserId, rawString);

    #endregion

    #region User私聊发送信息

    public static Task<IPrivateMessage> SendMessageAsync(this IUser user, long? groupId, IMessageEntity messageEntity)
        => user.Client.SendPrivateMessageAsync(user.UserId, groupId, messageEntity);

    public static Task<IPrivateMessage> SendMessageAsync(this IUser user, long? groupId, string rawString)
        => user.Client.SendPrivateMessageAsync(user.UserId, groupId, rawString);

    #endregion

    #region Group发送消息

    public static Task<IGroupMessage> SendMessageAsync(this IGroup group, IMessageEntity messageEntity)
        => group.Client.SendGroupMessageAsync(group.GroupId, messageEntity);

    public static Task<IGroupMessage> SendMessageAsync(this IGroup group, string rawString)
        => group.Client.SendGroupMessageAsync(group.GroupId, rawString);

    #endregion

    public static Task RecallAsync(this IMessage message)
        => message.Client.RecallMessageAsync(message.MessageId);

    #endregion

    #region 群操作

    #region 操作用户

    public static Task BanAsync(this IGroupUser groupUser, TimeSpan time)
        => groupUser.Client.BanGroupUserAsync(groupUser.Group.GroupId, groupUser.UserId, time);

    public static Task LiftBanAsync(this IGroupUser groupUser)
        => groupUser.Client.LiftBanGroupUserAsync(groupUser.Group.GroupId, groupUser.UserId);

    #endregion

    #endregion

    #region 实体额外操作

    public static bool IsFromFriends(this IPrivateMessage privateMessage)
        => privateMessage.TempSource == MessageTempSource.Invalid;


    #endregion
}
