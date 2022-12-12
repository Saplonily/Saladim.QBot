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

    public JoinedGroup Group => ExpGroup.Value;

    public new GroupUser Sender => ExpGroupSender.Value;

    public new GroupUser Author => Sender;

    public override ICqMessageWindow MessageWindow => Group;

    protected internal GroupMessage(CqClient client, int messageId)
        : base(client, messageId)
    {
        IsFromGroup = client.MakeNoneExpirableExpirable(true);
    }

    /// <summary>
    /// 回复一个群消息,
    /// 注意回复过程中不要再次引用消息实体,
    /// 回复过程中会向消息链前端加入回复节点
    /// </summary>
    /// <param name="msg">使用的消息实体</param>
    /// <returns>发送的消息</returns>
    public async Task<GroupMessage> ReplyAsync(MessageEntity msg)
    {
        msg.Chain.MessageChainNodes.Insert(0, new MessageChainReplyNode(Client, this));
        var sentMessage = await this.Group.SendMessageAsync(msg);
        msg.Chain.MessageChainNodes.RemoveAt(0);
        return sentMessage;
    }

    public async Task<GroupMessage> ReplyAsync(string rawString)
    {
        var newString = ((new MessageChainReplyNode(Client, this)).ToModel().CqStringify()) + rawString;
        var sentMessage = await this.Group.SendMessageAsync(newString);
        return sentMessage;
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