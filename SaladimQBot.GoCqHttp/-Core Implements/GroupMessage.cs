using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{DebuggerDisplay}")]
public class GroupMessage : Message, IGroupMessage
{
    public Expirable<JoinedGroup> ExpGroup { get; protected set; } = default!;

    public Expirable<GroupUser> ExpGroupSender { get; protected set; } = default!;

    public new Expirable<User> ExpSender { get; protected set; } = default!;

    public JoinedGroup Group => ExpGroup.Value;

    public GroupUser GroupSender => ExpGroupSender.Value;

    public new User Sender => ExpSender.Value;

    protected internal GroupMessage(ICqClient client, long messageId)
        : base(client, messageId)
    {
    }

    internal static GroupMessage CreateFromGroupMessagePost(ICqClient client, CqGroupMessagePost post)
        => new GroupMessage(client, post.MessageId)
                .LoadGroupAndSenderFromMessagePost(post);

    internal static new GroupMessage CreateFromMessageId(ICqClient client, long messageId)
        => new GroupMessage(client, messageId)
                .LoadGetMessageApiResult().Cast<GroupMessage>()
                .LoadGroupAndSenderFromMessageId();

    internal GroupMessage LoadGroupAndSenderFromMessageId()
    {
        base.LoadFromMessageId();
        ExpGroup = Client.MakeDependencyExpirable(ApiCallResult, GroupFactory).WithNoExpirable();
        ExpGroupSender = Client.MakeDependencyExpirable(ApiCallResult, GroupSenderFactory).WithNoExpirable();
        ExpSender = CastedExpirable<User, GroupUser>.MakeFromSource(ExpGroupSender);

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
        ExpSender = CastedExpirable<User, GroupUser>.MakeFromSource(ExpGroupSender);
        ExpGroup = Client.MakeNoneExpirableExpirable(JoinedGroup.CreateFromCqGroupMessagePost(Client, post));
        return this;
    }


    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IJoinedGroup IGroupMessage.Group { get => ExpGroup.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IGroupUser IGroupMessage.Sender { get => ExpGroupSender.Value; }
}
