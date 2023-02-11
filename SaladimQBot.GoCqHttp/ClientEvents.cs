using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class ClientEvent : IClientEvent
{
    public CqClient SourceClient { get; protected set; }

    IClient IClientEvent.SourceClient => SourceClient;

    public ClientEvent(CqClient sourceClient)
    {
        this.SourceClient = sourceClient;
    }
}

public class ClientTickEvent : ClientEvent, IClientTickEvent
{
    public ClientTickEvent(CqClient sourceClient) : base(sourceClient)
    {

    }
}

public class ClientMessageReceivedEvent : ClientEvent, IClientMessageReceivedEvent
{
    public Message Message { get; protected set; }

    public ClientMessageReceivedEvent(CqClient client, Message message)
        : base(client)
    {
        this.SourceClient = client;
        this.Message = message;
    }

    IMessage IClientMessageReceivedEvent.Message => Message;
}

public class ClientGroupMessageReceivedEvent : ClientMessageReceivedEvent, IClientGroupMessageReceivedEvent
{
    public new GroupMessage Message { get; protected set; }

    public GroupUser Sender { get; protected set; }

    public JoinedGroup Group { get; protected set; }

    public ClientGroupMessageReceivedEvent(CqClient client, GroupMessage message)
        : base(client, message)
    {
        Message = message;
        Sender = message.Sender;
        Group = message.Group;
    }

    IGroupMessage IClientGroupMessageReceivedEvent.Message => Message;
    IGroupUser IClientGroupMessageReceivedEvent.Sender => Sender;
    IJoinedGroup IClientGroupMessageReceivedEvent.Group => Group;
}

public class ClientPrivateMessageReceivedEvent : ClientMessageReceivedEvent, IClientPrivateMessageReceivedEvent
{
    public new PrivateMessage Message { get; protected set; }

    public ClientPrivateMessageReceivedEvent(CqClient client, PrivateMessage message)
        : base(client, message)
    {
        Message = message;
    }

    IPrivateMessage IClientPrivateMessageReceivedEvent.Message => Message;
}

public class ClientFriendMessageReceivedEvent : ClientPrivateMessageReceivedEvent, IClientFriendMessageReceivedEvent
{
    public new FriendMessage Message { get; protected set; }

    public FriendUser Sender { get; protected set; }

    public ClientFriendMessageReceivedEvent(CqClient client, FriendMessage message)
        : base(client, message)
    {
        Message = message;
        Sender = message.Sender;
    }

    IFriendMessage IClientFriendMessageReceivedEvent.Message => Message;
    IFriendUser IClientFriendMessageReceivedEvent.Sender => Sender;
}

public abstract class ClientMessageRecalledEvent : ClientEvent, IClientMessageRecalledEvent
{
    public Message Message { get; protected set; }

    public User MessageSender => lazyMessageSender.Value;

    protected Lazy<User> lazyMessageSender;

    public ClientMessageRecalledEvent(CqClient client, Message message)
        : base(client)
    {
        this.Message = message;
        lazyMessageSender = new(() => message.Sender, isThreadSafe: true);
    }

    IMessage IClientMessageRecalledEvent.Message => Message;

    IUser IClientMessageRecalledEvent.MessageSender => MessageSender;
}

public class ClientPrivateMessageRecalledEvent : ClientMessageRecalledEvent, IClientPrivateMessageRecalledEvent
{
    public new PrivateMessage Message { get; protected set; }

    public ClientPrivateMessageRecalledEvent(CqClient client, PrivateMessage message)
        : base(client, message)
    {
        this.Message = message;
    }

    IPrivateMessage IClientPrivateMessageRecalledEvent.Message => Message;
}

public class ClientGroupMessageRecalledEvent : ClientMessageRecalledEvent, IClientGroupMessageRecalledEvent
{
    public new GroupMessage Message { get; protected set; }

    public GroupUser Operator { get; protected set; }

    public ClientGroupMessageRecalledEvent(CqClient client, GroupMessage message, GroupUser @operator)
        : base(client, message)
    {
        this.Message = message;
        this.Operator = @operator;
    }

    IGroupMessage IClientGroupMessageRecalledEvent.Message => Message;
    IGroupUser IClientGroupMessageRecalledEvent.Operator => Operator;
}

public class ClientFriendAddedEvent : ClientEvent, IClientFriendAddedEvent
{
    public FriendUser FriendUser { get; protected set; }

    public ClientFriendAddedEvent(CqClient sourceClient, FriendUser friendUser)
        : base(sourceClient)
    {
        this.FriendUser = friendUser;
    }

    IFriendUser IClientFriendAddedEvent.FriendUser => FriendUser;
}

public abstract class ClientGroupAdminChangedEvent : ClientEvent, IClientGroupAdminChangedEvent
{
    public JoinedGroup Group { get; protected set; }

    public GroupUser User { get; protected set; }

    public bool IsSet { get; protected set; }

    public ClientGroupAdminChangedEvent(CqClient sourceClient, JoinedGroup group, GroupUser user, bool isSet)
        : base(sourceClient)
    {
        this.Group = group;
        this.User = user;
        this.IsSet = isSet;
    }

    IJoinedGroup IClientGroupAdminChangedEvent.Group => Group;
    IGroupUser IClientGroupAdminChangedEvent.User => User;
}

public class ClientGroupAdminSetEvent : ClientGroupAdminChangedEvent, IClientGroupAdminSetEvent
{
    public ClientGroupAdminSetEvent(CqClient sourceClient, JoinedGroup group, GroupUser user)
        : base(sourceClient, group, user, true)
    {
    }
}

public class ClientGroupAdminCancelledEvent : ClientGroupAdminChangedEvent, IClientGroupAdminCancelledEvent
{
    public ClientGroupAdminCancelledEvent(CqClient sourceClient, JoinedGroup group, GroupUser user)
        : base(sourceClient, group, user, false)
    {
    }
}

public abstract class ClientGroupEssenceSetEvent : ClientEvent, IClientGroupEssenceSetEvent
{
    public JoinedGroup Group { get; protected set; }

    public GroupUser Operator { get; protected set; }

    public GroupUser MessageSender { get; protected set; }

    public GroupMessage Message { get; protected set; }

    public bool IsAdd { get; protected set; }

    protected ClientGroupEssenceSetEvent(
        CqClient sourceClient,
        JoinedGroup group,
        GroupUser @operator,
        GroupUser messageSender,
        GroupMessage message,
        bool isAdd
        )
        : base(sourceClient)
    {
        this.Group = group;
        this.Operator = @operator;
        this.MessageSender = messageSender;
        this.Message = message;
        this.IsAdd = isAdd;
    }

    IJoinedGroup IClientGroupEssenceSetEvent.Group => Group;
    IGroupUser IClientGroupEssenceSetEvent.Operator => Operator;
    IGroupUser IClientGroupEssenceSetEvent.MessageSender => MessageSender;
    IGroupMessage IClientGroupEssenceSetEvent.Message => Message;
}

public class ClientGroupEssenceAddedEvent : ClientGroupEssenceSetEvent, IClientGroupEssenceAddedEvent
{
    public ClientGroupEssenceAddedEvent(
        CqClient sourceClient,
        JoinedGroup group,
        GroupUser @operator,
        GroupUser messageSender,
        GroupMessage message
        )
        : base(sourceClient, group, @operator, messageSender, message, true)
    {
    }
}

public class ClientGroupEssenceRemovedEvent : ClientGroupEssenceSetEvent, IClientGroupEssenceRemovedEvent
{
    public ClientGroupEssenceRemovedEvent(
        CqClient sourceClient,
        JoinedGroup group,
        GroupUser @operator,
        GroupUser messageSender,
        GroupMessage message
        )
        : base(sourceClient, group, @operator, messageSender, message, false)
    {
    }
}

public class ClientGroupFileUploadedEvent : ClientEvent, IClientGroupFileUploadedEvent
{
    public JoinedGroup Group { get; protected set; }

    public GroupUser Uploader { get; protected set; }

    public UploadedGroupFile GroupFile { get; protected set; }

    public ClientGroupFileUploadedEvent(
        CqClient sourceClient,
        JoinedGroup group,
        GroupUser uploader,
        UploadedGroupFile groupFile
        ) : base(sourceClient)
    {
        this.Group = group;
        this.Uploader = uploader;
        this.GroupFile = groupFile;
    }

    IJoinedGroup IClientGroupFileUploadedEvent.Group => Group;
    IGroupUser IClientGroupFileUploadedEvent.Uploader => Uploader;
    IUploadedGroupFile IClientGroupFileUploadedEvent.GroupFile => GroupFile;
}

public class ClientGroupMemberBannedEvent : ClientEvent, IClientGroupMemberBannedEvent
{
    public JoinedGroup Group { get; protected set; }

    public GroupUser Operator { get; protected set; }

    public GroupUser User { get; protected set; }

    public TimeSpan TimeSpan { get; protected set; }

    public ClientGroupMemberBannedEvent(
        CqClient sourceClient,
        JoinedGroup group,
        GroupUser @operator,
        GroupUser user,
        TimeSpan timeSpan
        ) : base(sourceClient)
    {
        this.Group = group;
        this.Operator = @operator;
        this.User = user;
        this.TimeSpan = timeSpan;
    }

    IJoinedGroup IClientGroupMemberBannedEvent.Group => Group;
    IGroupUser IClientGroupMemberBannedEvent.Operator => Operator;
    IGroupUser IClientGroupMemberBannedEvent.User => User;
}

public class ClientGroupMemberBanLiftedEvent : ClientEvent, IClientGroupMemberBanLiftedEvent
{
    public JoinedGroup Group { get; protected set; }

    public GroupUser Operator { get; protected set; }

    public GroupUser User { get; protected set; }

    public ClientGroupMemberBanLiftedEvent(
        CqClient sourceClient,
        JoinedGroup group,
        GroupUser @operator,
        GroupUser user
        ) : base(sourceClient)
    {
        this.Group = group;
        this.Operator = @operator;
        this.User = user;
    }

    IJoinedGroup IClientGroupMemberBanLiftedEvent.Group => Group;
    IGroupUser IClientGroupMemberBanLiftedEvent.Operator => Operator;
    IGroupUser IClientGroupMemberBanLiftedEvent.User => User;
}


public class ClientGroupAllUserBannedEvent : ClientEvent, IClientGroupAllUserBannedEvent
{
    public JoinedGroup Group { get; protected set; }

    public GroupUser Operator { get; protected set; }

    public ClientGroupAllUserBannedEvent(CqClient sourceClient, JoinedGroup group, GroupUser @operator) : base(sourceClient)
    {
        this.Group = group;
        this.Operator = @operator;
    }

    IJoinedGroup IClientGroupAllUserBannedEvent.Group => Group;
    IGroupUser IClientGroupAllUserBannedEvent.Operator => Operator;
}

public class ClientGroupAllUserBanLiftedEvent : ClientEvent, IClientGroupAllUserBanLiftedEvent
{
    public JoinedGroup Group { get; protected set; }

    public GroupUser Operator { get; protected set; }

    public ClientGroupAllUserBanLiftedEvent(CqClient sourceClient, JoinedGroup group, GroupUser @operator) : base(sourceClient)
    {
        this.Group = group;
        this.Operator = @operator;
    }

    IJoinedGroup IClientGroupAllUserBanLiftedEvent.Group => Group;
    IGroupUser IClientGroupAllUserBanLiftedEvent.Operator => Operator;
}

public class ClientGroupMemberCardChangedEvent : ClientEvent, IClientGroupMemberCardChangedEvent
{
    public JoinedGroup Group { get; protected set; }

    public GroupUser User { get; protected set; }

    public string From { get; protected set; }

    public string To { get; protected set; }

    public ClientGroupMemberCardChangedEvent(
        CqClient sourceClient,
        JoinedGroup group,
        GroupUser user,
        string from,
        string to
        ) : base(sourceClient)
    {
        this.Group = group;
        this.User = user;
        this.From = from;
        this.To = to;
    }

    IJoinedGroup IClientGroupMemberCardChangedEvent.Group => Group;
    IGroupUser IClientGroupMemberCardChangedEvent.User => User;
}

public class ClientOfflineFileReceivedEvent : ClientEvent, IClientOfflineFileReceivedEvent
{
    public User User { get; protected set; }

    public OfflineFile OfflineFile { get; protected set; }

    public ClientOfflineFileReceivedEvent(CqClient sourceClient, User user, OfflineFile offlineFile) : base(sourceClient)
    {
        this.User = user;
        this.OfflineFile = offlineFile;
    }

    IUser IClientOfflineFileReceivedEvent.User => User;
    IOfflineFile IClientOfflineFileReceivedEvent.OfflineFile => OfflineFile;
}

public class ClientGroupMemberChangedEvent : ClientEvent, IClientGroupMemberChangedEvent
{
    public JoinedGroup Group { get; protected set; }

    public User User { get; protected set; }

    public bool IsIncrease { get; protected set; }

    public ClientGroupMemberChangedEvent(CqClient sourceClient, JoinedGroup group, User user, bool isIncrease) : base(sourceClient)
    {
        this.Group = group;
        this.User = user;
        this.IsIncrease = isIncrease;
    }

    IJoinedGroup IClientGroupMemberChangedEvent.Group => Group;
    IUser IClientGroupMemberChangedEvent.User => User;
}

public class ClientGroupMemberIncreasedEvent : ClientGroupMemberChangedEvent, IClientGroupMemberIncreasedEvent
{
    public new GroupUser User { get; protected set; }

    public ClientGroupMemberIncreasedEvent(CqClient sourceClient, JoinedGroup group, GroupUser groupUser)
        : base(sourceClient, group, groupUser, true)
    {
        this.User = groupUser;
    }

    IGroupUser IClientGroupMemberIncreasedEvent.User => User;
}

public class ClientGroupMemberDecreasedEvent : ClientGroupMemberChangedEvent, IClientGroupMemberDecreasedEvent
{
    public ClientGroupMemberDecreasedEvent(CqClient sourceClient, JoinedGroup group, User user)
        : base(sourceClient, group, user, false)
    {
    }
}

public class ClientFriendAddRequestedEvent : ClientEvent, IClientFriendAddRequestedEvent
{
    public FriendAddRequest Request { get; protected set; }

    public ClientFriendAddRequestedEvent(CqClient sourceClient, FriendAddRequest request) : base(sourceClient)
    {
        this.Request = request;
    }

    IFriendAddRequest IClientFriendAddRequestedEvent.Request => Request;
}

public class ClientGroupJoinRequestedEvent : ClientEvent, IClientGroupJoinRequestedEvent
{
    public GroupJoinRequest Request { get; protected set; }

    public ClientGroupJoinRequestedEvent(CqClient sourceClient, GroupJoinRequest request) : base(sourceClient)
    {
        this.Request = request;
    }

    IGroupJoinRequest IClientGroupJoinRequestedEvent.Request => Request;
}

public class ClientGroupInviteRequestedEvent : ClientEvent, IClientGroupInviteRequestedEvent
{
    public GroupInviteRequest Request { get; protected set; }

    public ClientGroupInviteRequestedEvent(CqClient sourceClient, GroupInviteRequest request) : base(sourceClient)
    {
        this.Request = request;
    }

    IGroupInviteRequest IClientGroupInviteRequestedEvent.Request => Request;
}