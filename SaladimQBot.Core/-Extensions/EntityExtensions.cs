namespace SaladimQBot.Core;

public static class EntityExtensions
{
    #region User私聊发送消息

    public static Task<IPrivateMessage> SendMessageAsync(this IUser user, IMessageEntity messageEntity)
        => user.Client.SendPrivateMessageAsync(user.UserId, messageEntity);

    public static Task<IPrivateMessage> SendMessageAsync(this IUser user, string rawString)
        => user.Client.SendPrivateMessageAsync(user.UserId, rawString);

    #endregion

    #region Group发送消息

    public static Task<IGroupMessage> SendMessageAsync(this IGroup group, IMessageEntity messageEntity)
        => group.Client.SendGroupMessageAsync(group.GroupId, messageEntity);

    public static Task<IGroupMessage> SendMessageAsync(this IGroup group, string rawString)
        => group.Client.SendGroupMessageAsync(group.GroupId, rawString);

    #endregion
}
