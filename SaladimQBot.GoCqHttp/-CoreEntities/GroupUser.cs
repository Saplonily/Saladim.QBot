using System.Diagnostics;
using System.Reflection;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class GroupUser : User, IGroupUser
{
    public JoinedGroup Group { get; protected set; } = default!;

    #region Expirable的一些属性
    public new Expirable<string> Nickname { get; protected set; } = default!;

    public Expirable<string> Card { get; protected set; } = default!;

    public Expirable<string> Area { get; protected set; } = default!;

    public Expirable<DateTime> JoinTime { get; protected set; } = default!;

    public Expirable<DateTime> LastMessageSentTime { get; protected set; } = default!;

    public Expirable<string> GroupLevel { get; protected set; } = default!;

    public Expirable<GroupRole> GroupRole { get; protected set; } = default!;

    public Expirable<bool> IsUnFriendly { get; protected set; } = default!;

    public Expirable<string> GroupTitle { get; protected set; } = default!;

    public Expirable<DateTime> GroupTitleExpireTime { get; protected set; } = default!;

    public Expirable<bool> IsAbleToChangeCard { get; protected set; } = default!;

    public Expirable<DateTime> MuteExpireTime { get; protected set; } = default!;
    #endregion

    /// <summary>
    /// 返回 "Card (Nickname, UserId)" 或 "Nickname (UserId)" 样的字符串,
    /// 例如 "群废物 (Saplonily, 2748166392)" 和 "Saplonily (2748166392)"
    /// </summary>
    public string FullName
    {
        get => Card.Value != string.Empty ? $"{Card.Value} ({Nickname.Value}, {UserId})" : $"{Nickname.Value} ({UserId})";
    }

    /// <inheritdoc cref="FullName"/>
    public Task<string> FullNameAsync
    {
        get => Task.Run(() => FullName, CancellationToken.None);
    }

    protected new Expirable<GetGroupMemberInfoActionResultData> ApiCallResult { get; set; } = default!;

    protected GroupUser(CqClient client, long groupId, long userId)
        : base(client, userId)
    {
        Group = JoinedGroup.CreateFromGroupId(client, groupId);
    }

    public Task BanAsync(TimeSpan timeSpan)
        => Client.BanGroupUserAsync(this.Group.GroupId, this.UserId, timeSpan);

    public Task LiftBanAsync()
        => Client.LiftBanGroupUserAsync(this.Group.GroupId, this.UserId);

    #region 一堆杂七杂八的Load

    internal static GroupUser CreateFromCqGroupMessagePost(CqClient client, CqGroupMessagePost messagePost)
        => CreateFromCqGroupMessageSender(client, (CqGroupMessageSender)messagePost.Sender, messagePost.GroupId);

    internal static GroupUser CreateFromCqGroupMessageSender(CqClient client, CqGroupMessageSender groupSender, long groupId)
        => new GroupUser(client, groupId, groupSender.UserId)
                .LoadApiCallResult(groupId, groupSender.UserId)
                .LoadFromUserId()
                .LoadFromCqGroupMessageSender(groupSender)
                .LoadGroupFromGroupId(groupId);

    internal static GroupUser CreateFromGroupIdAndUserId(CqClient client, long groupId, long userId)
        => new GroupUser(client, groupId, userId)
                .LoadApiCallResult(groupId, userId)
                .LoadFromUserId()
                .LoadGroupFromGroupId(groupId);

    internal static GroupUser CreateFromGroupIdUserIdAndCard(CqClient client, long groupId, long userId, string card)
        => new GroupUser(client, groupId, userId)
                .LoadApiCallResult(groupId, userId)
                .LoadFromUserId()
                .LoadGroupFromGroupId(groupId)
                .LoadCard(card);

    protected internal GroupUser LoadCard(string card)
    {
        Card = Client.MakeDependencyExpirable(ApiCallResult, card, d => d.Card);
        return this;
    }

    protected internal GroupUser LoadFromCqGroupMessageSender(CqGroupMessageSender sender)
    {
        base.LoadFromMessageSender(sender);
        this.UserId = sender.UserId;
        var d = ApiCallResult;
        var c = Client;
        Card = c.MakeDependencyExpirable(d, sender.Card, d => d.Card);
        Area = c.MakeDependencyExpirable(d, sender.Area, d => d.Area);
        GroupRole = c.MakeDependencyExpirable(d, sender.Role, d => d.Role);
        GroupLevel = c.MakeDependencyExpirable(d, sender.Level, d => d.Level);
        GroupTitle = c.MakeDependencyExpirable(d, sender.Title, d => d.Title);
        //代码文本一模一样但是所使用的ApiCallResult类型不同
        Sex = Client.MakeDependencyExpirable(d, sender.Sex, d => d.Sex);
        Age = Client.MakeDependencyExpirable(d, sender.Age, d => d.Age);
        Nickname = Client.MakeDependencyExpirable(d, sender.Nickname, d => d.Nickname);
        return this;
    }

    protected internal new GroupUser LoadFromUserId()
    {
        base.LoadFromUserId();
        var d = ApiCallResult;
        var c = Client;
        Sex = c.MakeDependencyExpirable(d, d => d.Sex);
        Age = c.MakeDependencyExpirable(d, d => d.Age);
        Card = c.MakeDependencyExpirable(d, d => d.Card);
        Area = c.MakeDependencyExpirable(d, d => d.Area);
        JoinTime = c.MakeDependencyExpirable(d, d => DateTimeHelper.GetFromUnix(d.JoinTime));
        GroupRole = c.MakeDependencyExpirable(d, d => d.Role);
        GroupLevel = c.MakeDependencyExpirable(d, d => d.Level);
        GroupTitle = c.MakeDependencyExpirable(d, d => d.Title);
        IsUnFriendly = c.MakeDependencyExpirable(d, d => d.Unfriendly);
        Nickname = Client.MakeDependencyExpirable(d, d => d.Nickname);
        MuteExpireTime = c.MakeDependencyExpirable(d, d => DateTimeHelper.GetFromUnix(d.ShutUpTimeStamp));
        IsAbleToChangeCard = c.MakeDependencyExpirable(d, d => d.CardChangeable);
        LastMessageSentTime = c.MakeDependencyExpirable(d, d => DateTimeHelper.GetFromUnix(d.LastSentTime));
        GroupTitleExpireTime = c.MakeDependencyExpirable(d, d => DateTimeHelper.GetFromUnix(d.TitleExpireTime));
        return this;
    }

    protected internal GroupUser LoadGroupFromGroupId(long groupId)
    {
        this.Group = JoinedGroup.CreateFromGroupId(Client, groupId);
        return this;
    }

    protected internal GroupUser LoadApiCallResult(long groupId, long userId)
    {
        base.LoadApiCallResult(userId);
        CqApi api = new GetGroupMemberInfoAction()
        {
            GroupId = groupId,
            UserId = userId,
            UseCache = true
        };
        ApiCallResult = Client.MakeExpirableApiCallResultData<GetGroupMemberInfoActionResultData>(api);
        return this;
    }

    #endregion

    #region IGroupUser

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IJoinedGroup IGroupUser.Group { get => Group; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IGroupUser.Card { get => Card.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IGroupUser.Area { get => Area.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime IGroupUser.JoinTime { get => JoinTime.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime IGroupUser.LastMessageSentTime { get => LastMessageSentTime.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IGroupUser.GroupLevel { get => GroupLevel.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Core.GroupRole IGroupUser.GroupRole { get => GroupRole.Value.Cast<Core.GroupRole>(); }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool IGroupUser.IsUnFriendly { get => IsUnFriendly.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IGroupUser.GroupTitle { get => GroupTitle.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime IGroupUser.GroupTitleExpireTime { get => GroupTitleExpireTime.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool IGroupUser.IsAbleToChangeCard { get => IsAbleToChangeCard.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime IGroupUser.MuteExpireTime { get => MuteExpireTime.Value; }


    #endregion

    public override bool Equals(object? obj)
    {
        return obj is GroupUser user &&
               base.Equals(obj) &&
               this.UserId == user.UserId &&
               EqualityComparer<JoinedGroup>.Default.Equals(this.Group, user.Group);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), this.UserId, this.Group);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => FullName;

    public static bool operator ==(GroupUser? left, GroupUser? right)
    {
        return EqualityComparer<GroupUser>.Default.Equals(left!, right!);
    }

    public static bool operator !=(GroupUser? left, GroupUser? right)
    {
        return !(left == right);
    }
}