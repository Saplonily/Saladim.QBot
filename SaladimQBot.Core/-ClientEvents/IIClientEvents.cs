namespace SaladimQBot.Core;

public interface IIClientEvent
{
    IClient SourceClient { get; }
}

public interface IClientMessageReceivedEvent : IIClientEvent
{
    IMessage Message { get; }
}

public interface IClientGroupMessageReceivedEvent : IClientMessageReceivedEvent
{
    new IGroupMessage Message { get; }

    IGroupUser Sender { get; }

    IJoinedGroup Group { get; }
}

public interface IClientPrivateMessageReceivedEvent : IClientMessageReceivedEvent
{
    new IPrivateMessage Message { get; }
}

public interface IClientFriendMessageReceivedEvent : IClientPrivateMessageReceivedEvent
{
    new IFriendMessage Message { get; }

    IFriendUser Sender { get; }
}

public interface IClientMessageRecalledEvent : IIClientEvent
{
    IMessage Message { get; }

    IUser MessageSender { get; }
}

public interface IClientPrivateMessageRecalledEvent : IClientMessageRecalledEvent
{
    new IPrivateMessage Message { get; }
}

public interface IClientGroupMessageRecalledEvent : IClientMessageRecalledEvent
{
    new IGroupMessage Message { get; }

    IGroupUser Operator { get; }
}

public interface IClientFriendAddedEvent : IIClientEvent
{
    IFriendUser FriendUser { get; }
}

public interface IClientGroupAdminChangedEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser User { get; }

    bool IsSet { get; }
}

public interface IClientGroupAdminSetEvent : IClientGroupAdminChangedEvent
{
}

public interface IClientGroupAdminCancelledEvent : IClientGroupAdminChangedEvent
{
}

public interface IClientGroupEssenceSetEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }

    IGroupUser MessageSender { get; }

    IGroupMessage Message { get; }

    bool IsAdd { get; }
}

public interface IClientGroupEssenceAddedEvent : IClientGroupEssenceSetEvent
{
}

public interface IClientGroupEssenceRemovedEvent : IClientGroupEssenceSetEvent
{
}

public interface IClientGroupFileUploadedEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Uploader { get; }

    IUploadedGroupFile GroupFile { get; }
}

public interface IClientGroupMemberBannedEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }

    IGroupUser User { get; }

    TimeSpan TimeSpan { get; }
}

public interface IClientGroupMemberBanLiftedEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }

    IGroupUser User { get; }
}


public interface IClientGroupAllUserBannedEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }
}

public interface IClientGroupAllUserBanLiftedEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser Operator { get; }
}

public interface IClientGroupMemberCardChangedEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IGroupUser User { get; }

    string From { get; }

    string To { get; }
}

public interface IClientOfflineFileReceivedEvent : IIClientEvent
{
    IUser User { get; }

    IOfflineFile OfflineFile { get; }
}

public interface IClientGroupMemberChangedEvent : IIClientEvent
{
    IJoinedGroup Group { get; }

    IUser User { get; }

    bool IsIncrease { get; }
}

public interface IClientGroupMemberIncreasedEvent : IClientGroupMemberChangedEvent
{
    new IGroupUser User { get; }
}

public interface IClientGroupMemberDecreasedEvent : IClientGroupMemberChangedEvent
{
}

public interface IClientFriendAddRequestedEvent : IIClientEvent
{
    IFriendAddRequest Request { get; }
}

public interface IClientGroupJoinRequestedEvent : IIClientEvent
{
    IGroupJoinRequest Request { get; }
}

public interface IClientGroupInviteRequestedEvent : IIClientEvent
{
    IGroupInviteRequest Request { get; }
}