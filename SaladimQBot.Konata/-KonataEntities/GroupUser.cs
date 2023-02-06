using System;
using Konata.Core.Common;
using Konata.Core.Interfaces.Api;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.Konata;

// wip, do not use
public class GroupUser : User, IGroupUser
{

    public IJoinedGroup Group { get; }

    public string Card { get; }

    public string Area { get; }

    public DateTime JoinTime { get; }

    public DateTime LastMessageSentTime { get; }

    public string GroupLevel { get; }

    public GroupRole GroupRole { get; }

    public bool IsUnFriendly { get; }

    public string GroupTitle { get; }

    public DateTime GroupTitleExpireTime { get; }

    public bool IsAbleToChangeCard { get; }

    public DateTime MuteExpireTime { get; }

    public override Sex Sex { get; }

    public override int Age { get; }

    public override int Level { get; }

    public override int LoginDays { get; }

    protected internal IndependentExpirable<BotMember> getMemberApi;

    public GroupUser(KqClient client, long userId, long groupId, string nickname)
        : base(client, userId, nickname)
    {
        
        getMemberApi = new(BotMemberFactory, client.ExpireTimeSpan);
    }

    protected BotMember BotMemberFactory()
    {
        Client.konatoBot.GetGroupMemberInfo()
            Client.konatoBot.GetFr
    }
}
