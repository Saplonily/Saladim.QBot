﻿using System.Diagnostics;
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

    public override ICqMessageWindow MessageWindow => Group;

    public JoinedGroup Group => ExpGroup.Value;

    public new GroupUser Sender => ExpGroupSender.Value;

    public new GroupUser Author => Sender;

    protected internal GroupMessage(ICqClient client, int messageId)
        : base(client, messageId)
    {
    }

    internal static GroupMessage CreateFromGroupMessagePost(ICqClient client, CqGroupMessagePost post)
        => new GroupMessage(client, post.MessageId)
                .LoadGroupAndSenderFromMessagePost(post);

    internal static new GroupMessage CreateFromMessageId(ICqClient client, int messageId)
        => new GroupMessage(client, messageId)
                .LoadGetMessageApiResult().Cast<GroupMessage>()
                .LoadGroupAndSenderFromMessageId();

    internal GroupMessage LoadGroupAndSenderFromMessageId()
    {
        base.LoadFromMessageId();
        ExpGroup = Client.MakeDependencyExpirable(ApiCallResult, GroupFactory).WithNoExpirable();
        ExpGroupSender = Client.MakeDependencyExpirable(ApiCallResult, GroupSenderFactory).WithNoExpirable();

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
        ExpGroup = Client.MakeNoneExpirableExpirable(JoinedGroup.CreateFromCqGroupMessagePost(Client, post));
        return this;
    }


    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IJoinedGroup IGroupMessage.Group { get => ExpGroup.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IGroupUser IGroupMessage.Sender { get => ExpGroupSender.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IMessageWindow IMessage.MessageWindow => Group;

}