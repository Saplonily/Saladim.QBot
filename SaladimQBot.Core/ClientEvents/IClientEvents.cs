namespace SaladimQBot.Core;

/// <summary>
/// Client发生的事件
/// </summary>
public interface IClientEvent
{
    IClient SourceClient { get; }
}

/// <summary>
/// Client tick事件, 每秒发生一次
/// </summary>
public interface IClientTickEvent : IClientEvent
{
}

/// <summary>
/// Client 消息收到事件
/// </summary>
public interface IClientMessageReceivedEvent : IClientEvent
{
    IMessage Message { get; }
}

/// <summary>
/// Client 群消息收到事件
/// </summary>
public interface IClientGroupMessageReceivedEvent : IClientMessageReceivedEvent
{
    new IGroupMessage Message { get; }

    IGroupUser Sender { get; }

    IJoinedGroup Group { get; }
}

/// <summary>
/// Client 私聊消息收到事件
/// </summary>
public interface IClientPrivateMessageReceivedEvent : IClientMessageReceivedEvent
{
    new IPrivateMessage Message { get; }
}

/// <summary>
/// Client 好友消息收到事件
/// </summary>
public interface IClientFriendMessageReceivedEvent : IClientPrivateMessageReceivedEvent
{
    new IFriendMessage Message { get; }

    IFriendUser Sender { get; }
}

/// <summary>
/// Client 消息撤回事件
/// </summary>
public interface IClientMessageRecalledEvent : IClientEvent
{
    IMessage Message { get; }

    IUser MessageSender { get; }
}

/// <summary>
/// Client 私聊消息撤回事件
/// </summary>
public interface IClientPrivateMessageRecalledEvent : IClientMessageRecalledEvent
{
    new IPrivateMessage Message { get; }
}

/// <summary>
/// Client 群消息撤回事件
/// </summary>
public interface IClientGroupMessageRecalledEvent : IClientMessageRecalledEvent
{
    new IGroupMessage Message { get; }

    IGroupUser Operator { get; }
}

/// <summary>
/// Client 好友添加事件
/// </summary>
public interface IClientFriendAddedEvent : IClientEvent
{
    IFriendUser FriendUser { get; }
}

/// <summary>
/// Client 群管理员变更事件
/// </summary>
public interface IClientGroupAdminChangedEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser User { get; }

    bool IsSet { get; }
}

/// <summary>
/// Client 群管理员设置事件
/// </summary>
public interface IClientGroupAdminSetEvent : IClientGroupAdminChangedEvent
{
}

/// <summary>
/// Client 群管理员取消事件
/// </summary>
public interface IClientGroupAdminCancelledEvent : IClientGroupAdminChangedEvent
{
}

/// <summary>
/// Client 群精华消息变更事件
/// </summary>
public interface IClientGroupEssenceSetEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }

    IGroupUser MessageSender { get; }

    IGroupMessage Message { get; }

    bool IsAdd { get; }
}

/// <summary>
/// Client 群精华消息增加
/// </summary>
public interface IClientGroupEssenceAddedEvent : IClientGroupEssenceSetEvent
{
}

/// <summary>
/// Client 群精华消息移除
/// </summary>
public interface IClientGroupEssenceRemovedEvent : IClientGroupEssenceSetEvent
{
}

/// <summary>
/// Client 群文件上传事件
/// </summary>
public interface IClientGroupFileUploadedEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Uploader { get; }

    IUploadedGroupFile GroupFile { get; }
}

/// <summary>
/// Client 群成员禁言事件
/// </summary>
public interface IClientGroupMemberBannedEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }

    IGroupUser User { get; }

    TimeSpan TimeSpan { get; }
}

/// <summary>
/// Client 群成员解禁事件
/// </summary>
public interface IClientGroupMemberBanLiftedEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }

    IGroupUser User { get; }
}

/// <summary>
/// Client 群全体禁言事件
/// </summary>
public interface IClientGroupAllUserBannedEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }
}

/// <summary>
/// Client 群全体禁言解除事件
/// </summary>
public interface IClientGroupAllUserBanLiftedEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }
}

/// <summary>
/// Client 群成员名片变更
/// </summary>
public interface IClientGroupMemberCardChangedEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser User { get; }

    string From { get; }

    string To { get; }
}

/// <summary>
/// Client 收到离线文件事件
/// </summary>
public interface IClientOfflineFileReceivedEvent : IClientEvent
{
    IUser User { get; }

    IOfflineFile OfflineFile { get; }
}

/// <summary>
/// Client 群成员变动事件
/// </summary>
public interface IClientGroupMemberChangedEvent : IClientEvent
{
    IJoinedGroup Group { get; }

    IUser User { get; }

    bool IsIncrease { get; }
}

/// <summary>
/// Client 群成员增加事件
/// </summary>
public interface IClientGroupMemberIncreasedEvent : IClientGroupMemberChangedEvent
{
    new IGroupUser User { get; }
}

/// <summary>
/// Client 群成员减少事件
/// </summary>
public interface IClientGroupMemberDecreasedEvent : IClientGroupMemberChangedEvent
{
}

/// <summary>
/// Client 好友添加请求事件
/// </summary>
public interface IClientFriendAddRequestedEvent : IClientEvent
{
    IFriendAddRequest Request { get; }
}

/// <summary>
/// Client 群加入请求事件
/// </summary>
public interface IClientGroupJoinRequestedEvent : IClientEvent
{
    IGroupJoinRequest Request { get; }
}

/// <summary>
/// Client 被邀请加入群请求事件
/// </summary>
public interface IClientGroupInviteRequestedEvent : IClientEvent
{
    IGroupInviteRequest Request { get; }
}