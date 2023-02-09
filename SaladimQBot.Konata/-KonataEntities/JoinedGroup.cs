using System.Collections.Generic;
using Konata.Core.Common;
using Konata.Core.Interfaces.Api;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.Konata;

public class JoinedGroup : Group, IJoinedGroup
{
    public IExpirable<int> MembersCount { get; }

    public IExpirable<int> MaxMembersCount { get; }

    public IExpirable<string> ExpName { get; }

    public IExpirable<IEnumerable<IGroupUser>> Members { get; }

    protected IDependencyExpirable<BotGroup> expGroup;

    protected internal JoinedGroup(KqClient client, long groupId) : base(client, groupId)
    {
        expGroup = DependencyExpirable<BotGroup>.Create(client.expBotGroups, g =>
        {
            var group = client.expBotGroups.Value.FirstOrDefault(g => g.Uin == groupId);
            return group ?? throw new NotSupportedException("Bot is not in the target group.");
        });
        MembersCount = DependencyExpirable<int>.Create(expGroup, g => (int)g.MemberCount);
        MaxMembersCount = DependencyExpirable<int>.Create(expGroup, g => (int)g.MaxMemberCount);
        ExpName = DependencyExpirable<string>.Create(expGroup, g => g.Name);
        Members = DependencyExpirable<IEnumerable<IGroupUser>>.Create(expGroup, g =>
            client.konataBot.GetGroupMemberList(expGroup.Value.Uin)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult()
                            .Select(botMember => new GroupUser(client, botMember.Uin, g.Uin, g.Name))
        );
    }

    public async Task<IMessage> SendMessageAsync(IMessageEntity messageEntity)
        => await Client.SendGroupMessageAsync(GroupId, messageEntity).ConfigureAwait(false);

    public async Task<IMessage> SendMessageAsync(string rawString)
        => await Client.SendGroupMessageAsync(GroupId, rawString).ConfigureAwait(false);

    public async Task<IMessage> SendMessageAsync(IForwardEntity forwardEntity)
        => await Client.SendGroupMessageAsync(GroupId, forwardEntity).ConfigureAwait(false);

    DateTime IJoinedGroup.CreateTime => throw new NotSupportedException("Konata does not support getting group create time.");

    uint IJoinedGroup.GroupLevel => throw new NotSupportedException("Konata does not support getting group level.");

    int IJoinedGroup.MembersCount => MembersCount.Value;

    int IJoinedGroup.MaxMembersCount => MaxMembersCount.Value;

    IEnumerable<IGroupUser> IJoinedGroup.Members => Members.Value;

    public override string Name => ExpName.Value;
}
