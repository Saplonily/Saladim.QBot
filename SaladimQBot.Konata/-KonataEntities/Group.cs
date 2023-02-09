using System;
using Konata.Core.Common;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.Konata;

public class Group : KqEntity, IGroup
{
    public long GroupId { get; }

    public virtual string Name => throw new NotSupportedException("Konata does not support get name of the group that didn't joined.");

    public string Remark => throw new NotSupportedException("Konata does not support get group remark.");

    public Uri AvatarUrl => new($"https://p.qlogo.cn/gh/{GroupId}/{GroupId}/100");

    protected internal Group(KqClient client, long groupId) : base(client)
    {
        GroupId = groupId;
    }
}
