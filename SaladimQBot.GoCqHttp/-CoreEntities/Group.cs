﻿using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

public class Group : CqEntity, IGroup
{
    public long GroupId { get; protected set; }

    public Expirable<string> Name { get; protected set; } = default!;

    public Expirable<string> Remark { get; protected set; } = default!;

    protected Expirable<GetGroupInfoActionResultData> ApiCallResultData { get; set; } = default!;

    protected internal Group(CqClient client, long groupId) : base(client)
    {
        GroupId = groupId;
    }

    #region CreateFrom / LoadFrom集合

    internal static Group CreateFromCqGroupMessagePost(CqClient client, CqGroupMessagePost post)
        => CreateFromGroupId(client, post.GroupId);

    internal static Group CreateFromGroupId(CqClient client, long groupId)
        => new Group(client, groupId).LoadFromGroupId(groupId);

    protected internal Group LoadFromGroupId(long groupId)
    {
        var api = new GetGroupInfoAction() { GroupId = groupId };
        var d = ApiCallResultData = Client.MakeExpirableApiCallResultData<GetGroupInfoActionResultData>(api);
        Name = Client.MakeDependencyExpirable(d, d => d.GroupName);
        Remark = Client.MakeDependencyExpirable(d, d => d.GroupMemo);
        return this;
    }

    #endregion

    #region IGroup

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IGroup.Name { get => Name.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IGroup.Remark { get => Remark.Value; }

    #endregion

    #region equals重写

    public override bool Equals(object? obj)
    {
        return obj is Group group &&
               this.GroupId == group.GroupId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.GroupId);
    }

    public static bool operator ==(Group? left, Group? right)
    {
        return EqualityComparer<Group>.Default.Equals(left, right);
    }

    public static bool operator !=(Group? left, Group? right)
    {
        return !(left == right);
    }


    #endregion
}