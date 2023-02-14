using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{Sender.Nickname}({Sender.UserId}): {MessageEntity.RawString}")]
public class GroupMessage : Message, IGroupMessage
{
    public IExpirable<JoinedGroup> ExpGroup { get; protected set; } = default!;

    public IExpirable<GroupUser> ExpGroupSender { get; protected set; } = default!;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public JoinedGroup Group => ExpGroup.Value;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public new GroupUser Sender => ExpGroupSender.Value;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public new GroupUser Author => Sender;

    public override ICqMessageWindow MessageWindow => Group;

    protected internal GroupMessage(CqClient client, int messageId)
        : base(client, messageId)
    {
        IsFromGroup = client.MakeNoneExpirableExpirable(true);
    }

    #region load一大堆的

    internal static GroupMessage CreateFromGroupMessagePost(CqClient client, CqGroupMessagePost post)
        => new GroupMessage(client, post.MessageId)
                .LoadGroupAndSenderFromMessagePost(post);

    internal static new GroupMessage CreateFromMessageId(CqClient client, int messageId)
        => new GroupMessage(client, messageId)
                .LoadGetMessageApiResult().Cast<GroupMessage>()
                .LoadGroupAndSenderFromMessageId();

    internal GroupMessage LoadGroupAndSenderFromMessageId()
    {
        base.LoadFromMessageId();
        ExpGroup = Client.MakeDependencyExpirable(ApiCallResult, GroupFactory);
        ExpGroupSender = Client.MakeDependencyExpirable(ApiCallResult, GroupSenderFactory);
        ExpSender = ExpGroupSender;


        return this;
        JoinedGroup GroupFactory(GetMessageActionResultData d)
        {
            if (d.IsGroupMessage == false)
                throw new InvalidOperationException("The message isn't a group message.");
            long groupId = (long)d.GroupId!;
            return JoinedGroup.CreateFromGroupId(Client, groupId);
        }
        GroupUser GroupSenderFactory(GetMessageActionResultData d)
        {
            if (d.IsGroupMessage == false)
                throw new InvalidOperationException("The message isn't a group message.");
            long groupId = (long)d.GroupId!;
            return GroupUser.CreateFromGroupIdUserIdAndCard(Client, groupId, d.Sender.UserId, d.Sender.Nickname);
        }
    }

    internal GroupMessage LoadGroupAndSenderFromMessagePost(CqGroupMessagePost post)
    {
        base.LoadFromMessagePost(post);
        ExpGroupSender = Client.MakeNoneExpirableExpirable(GroupUser.CreateFromCqGroupMessagePost(Client, post));
        ExpSender = ExpGroupSender;
        ExpGroup = Client.MakeNoneExpirableExpirable(JoinedGroup.CreateFromCqGroupMessagePost(Client, post));
        return this;
    }

    #endregion

    public override bool Equals(object? obj)
    {
        return obj is GroupMessage message &&
               base.Equals(obj) &&
               EqualityComparer<JoinedGroup>.Default.Equals(this.Group, message.Group) &&
               EqualityComparer<GroupUser>.Default.Equals(this.Sender, message.Sender);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), this.Group, this.Sender);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IJoinedGroup IGroupMessage.Group { get => ExpGroup.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IGroupUser IGroupMessage.Sender { get => ExpGroupSender.Value; }

    public static bool operator ==(GroupMessage? left, GroupMessage? right)
    {
        return EqualityComparer<GroupMessage>.Default.Equals(left!, right!);
    }

    public static bool operator !=(GroupMessage? left, GroupMessage? right)
    {
        return !(left == right);
    }
}