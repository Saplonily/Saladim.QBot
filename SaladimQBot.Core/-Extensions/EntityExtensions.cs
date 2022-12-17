namespace SaladimQBot.Core;

public static class EntityExtensions
{
    #region 消息相关

    #region FriendUser私聊发送消息

    public static Task<IFriendMessage> SendMessageAsync(this IFriendUser user, IMessageEntity messageEntity)
        => user.Client.SendFriendMessageAsync(user.UserId, messageEntity);

    public static Task<IFriendMessage> SendMessageAsync(this IFriendUser user, string rawString)
        => user.Client.SendFriendMessageAsync(user.UserId, rawString);

    public static Task<IFriendMessage> SendMessageAsync(this IFriendUser user, IForwardEntity forwardEntity)
        => user.Client.SendFriendMessageAsync(user.UserId, forwardEntity);

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

    #region 额外属性

    public static string GetFullName(this IGroupUser groupUser)
        => groupUser.Card is "" ? $"{groupUser.Nickname}({groupUser.UserId})" : $"{groupUser.Card}({groupUser.Nickname}, {groupUser.UserId})";

    public static string GetCardOrName(this IGroupUser groupUser)
        => groupUser.Card is "" ? groupUser.Nickname : groupUser.Card;

    #endregion

    #region 消息实体相关

    public static T First<T>(this IMessageEntity entity) where T : IMessageChainNode
        => (T)entity.Chain.ChainNodes.First(n => n is T);

    public static T? FirstOrNull<T>(this IMessageEntity entity) where T : IMessageChainNode
    => (T?)entity.Chain.ChainNodes.FirstOrDefault(n => n is T);

    public static IEnumerable<T> AllOf<T>(this IMessageEntity entity) where T : IMessageChainNode
        => entity.Chain.ChainNodes.Where(n => n is T).Select(n => (T)n);

    public static IMessageChainAtNode FirstAt(this IMessageEntity entity)
        => entity.First<IMessageChainAtNode>();

    public static IMessageChainForwardNode FirstForward(this IMessageEntity entity)
        => entity.First<IMessageChainForwardNode>();

    public static IMessageChainImageNode FirstImage(this IMessageEntity entity)
        => entity.First<IMessageChainImageNode>();

    public static IMessageChainReplyNode FirstReply(this IMessageEntity entity)
        => entity.First<IMessageChainReplyNode>();

    public static IMessageChainTextNode FirstText(this IMessageEntity entity)
        => entity.First<IMessageChainTextNode>();

    public static IMessageChainAtNode? FirstAtOrNull(this IMessageEntity entity)
        => entity.FirstOrNull<IMessageChainAtNode>();

    public static IMessageChainForwardNode? FirstForwardOrNull(this IMessageEntity entity)
        => entity.FirstOrNull<IMessageChainForwardNode>();

    public static IMessageChainImageNode? FirstImageOrNull(this IMessageEntity entity)
        => entity.FirstOrNull<IMessageChainImageNode>();

    public static IMessageChainReplyNode? FirstReplyOrNull(this IMessageEntity entity)
        => entity.FirstOrNull<IMessageChainReplyNode>();

    public static IMessageChainTextNode? FirstTextOrNull(this IMessageEntity entity)
        => entity.FirstOrNull<IMessageChainTextNode>();

    /// <summary>
    /// 消息的所有@
    /// </summary>
    public static IEnumerable<IMessageChainAtNode> AllAt(this IMessageEntity entity)
        => entity.AllOf<IMessageChainAtNode>();

    public static IEnumerable<IMessageChainImageNode> AllImage(this IMessageEntity entity)
        => entity.AllOf<IMessageChainImageNode>();

    public static IEnumerable<IMessageChainTextNode> AllText(this IMessageEntity entity)
        => entity.AllOf<IMessageChainTextNode>();

    /// <summary>
    /// 消息是否提及某个用户 (不包含@全体成员)
    /// </summary>
    /// <param name="user">目标用户</param>
    public static bool Mentioned(this IMessageEntity entity, IUser user)
        => entity.AllAt().Where(n => !n.MentionedAllUser() && n.User! == user).Any();

    public static bool MentionedAllUser(this IMessageEntity entity)
        => entity.AllAt().Where(n => n.MentionedAllUser()).Any();

    /// <summary>
    /// 消息是否@了bot (不包含@全体成员)
    /// </summary>
    public static bool MentionedSelf(this IMessageEntity entity)
        => entity.Mentioned(entity.Client.Self);

    /// <summary>
    /// <para>是否该消息中包含回复该消息</para>
    /// </summary>
    /// <param name="message">消息实体</param>
    /// <returns>消息是否被回复</returns>
    public static bool Replied(this IMessageEntity entity, IMessage message)
    {
        var n = entity.FirstReplyOrNull();
        if (n is null) return false;
        if (n.MessageBeReplied == message) return true;
        return false;
    }

    public static bool MentionedAllUser(this IMessageChainAtNode atNode)
    {
        return atNode.User is null;
    }

    #endregion
}
