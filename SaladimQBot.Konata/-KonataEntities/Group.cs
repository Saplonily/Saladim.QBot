using System;
using SaladimQBot.Core;

namespace SaladimQBot.Konata;

public class Group : KqEntity, IGroup
{
    public long GroupId { get; }

    public string Name { get; }

    public string Remark { get; }

    public Uri AvatarUrl { get; }

    public Group(KqClient client) : base(client)
    {

    }
}
