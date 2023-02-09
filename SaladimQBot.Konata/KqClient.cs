using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using SaladimQBot.Core;
using SaladimQBot.Shared;
using SqlSugar;

namespace SaladimQBot.Konata;

public class KqClient : IClient
{
    protected SqlSugarScope sugarScope;
    internal Bot konataBot;
    internal readonly SourceExpirable<IReadOnlyList<BotFriend>> expBotFriends;
    internal readonly SourceExpirable<IReadOnlyList<BotGroup>> expBotGroups;
    internal readonly Dictionary<(long GroupId, long UserId), IndependentExpirable<BotMember>> expBotMembers;

    public TimeSpan ExpireTimeSpan = TimeSpan.FromMinutes(5);
    public TimeSpan FriendGroupListExpireTimeSpan = TimeSpan.FromMinutes(13);

    public event IClient.OnClientEventOccuredHandler<IClientEvent>? OnClientEventOccurred;

    public event Action<Exception>? OnStoppedUnexpectedly;

    public IUser Self => throw new NotImplementedException();

    public KqClient(Bot konataBot)
    {
        sugarScope = new(new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=SaladimQBot.KonataMessages.db"
        });
        sugarScope.CodeFirst.InitTables<MessageStoraged>();

        expBotMembers = new();

        this.konataBot = konataBot;
        expBotFriends = new(new(() => konataBot.GetFriendList().ConfigureAwait(false).GetAwaiter().GetResult(), FriendGroupListExpireTimeSpan));
        expBotGroups = new(new(() => konataBot.GetGroupList().ConfigureAwait(false).GetAwaiter().GetResult(), FriendGroupListExpireTimeSpan));
        konataBot.OnGroupMessage += this.KonatoBot_OnGroupMessage;
    }

    private void KonatoBot_OnGroupMessage(Bot sender, GroupMessageEvent args)
    {

    }

    internal IndependentExpirable<BotMember> GetExpirableBotMember(long groupId, long userId)
    {
        if (expBotMembers.TryGetValue((groupId, userId), out var member))
        {
            return member;
        }
        else
        {
            var newMember = new IndependentExpirable<BotMember>(Factory, ExpireTimeSpan);
            BotMember Factory()
                => konataBot.GetGroupMemberInfo((uint)groupId, (uint)userId).ConfigureAwait(false).GetAwaiter().GetResult();
            expBotMembers.Add((groupId, userId), newMember);
            return newMember;
        }
    }

    public Task StartAsync()
    {
        throw new NotImplementedException();
    }

    public Task StopAsync()
    {
        throw new NotImplementedException();
    }

    #region IClient交互
    public IGroupMessage GetGroupMessageById(int messageId)
    {
        throw new NotImplementedException();
    }

    public IPrivateMessage GetPrivateMessageById(int messageId)
    {
        throw new NotImplementedException();
    }

    public IFriendMessage GetFriendMessageById(int messageId)
    {
        throw new NotImplementedException();
    }

    public IMessage GetMessageById(int messageId)
    {
        throw new NotImplementedException();
    }

    public Task<IPrivateMessage> SendPrivateMessageAsync(long userId, long? groupId, IMessageEntity messageEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IPrivateMessage> SendPrivateMessageAsync(long userId, long? groupId, string rawString)
    {
        throw new NotImplementedException();
    }

    public Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, IMessageEntity messageEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, string rawString)
    {
        throw new NotImplementedException();
    }

    public Task<IGroupMessage> SendGroupMessageAsync(long groupId, IMessageEntity messageEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IGroupMessage> SendGroupMessageAsync(long groupId, string rawString)
    {
        throw new NotImplementedException();
    }

    public Task<IGroupMessage> SendGroupMessageAsync(long groupId, IForwardEntity forwardEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IPrivateMessage> SendPrivateMessageAsync(long userId, IForwardEntity forwardEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, IForwardEntity forwardEntity)
    {
        throw new NotImplementedException();
    }

    public Task RecallMessageAsync(int messageId)
    {
        throw new NotImplementedException();
    }

    public Task BanGroupUserAsync(long groupId, long userId, TimeSpan time)
    {
        throw new NotImplementedException();
    }

    public Task LiftBanGroupUserAsync(long groupId, long userId)
    {
        throw new NotImplementedException();
    }

    public Task SetGroupNameAsync(long groupId, string newGroupName)
    {
        throw new NotImplementedException();
    }

    public Task SetGroupCardAsync(long groupId, long userId, string newCard)
    {
        throw new NotImplementedException();
    }

    public IGroupUser GetGroupUser(long groupId, long userId)
    {
        throw new NotImplementedException();
    }

    public IUser GetUser(long userId)
    {
        throw new NotImplementedException();
    }

    public IGroup GetGroup(long groupId)
    {
        throw new NotImplementedException();
    }

    public IJoinedGroup GetJoinedGroup(long groupId)
    {
        throw new NotImplementedException();
    }

    public IFriendUser GetFriendUser(long friendUserId)
    {
        throw new NotImplementedException();
    }

    public IMessageEntityBuilder CreateMessageBuilder()
    {
        throw new NotImplementedException();
    }

    public IForwardEntityBuilder CreateForwardBuilder()
    {
        throw new NotImplementedException();
    }
    #endregion
}
