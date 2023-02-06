using System;
using System.Threading.Tasks;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using SaladimQBot.Core;
using SqlSugar;

namespace SaladimQBot.Konata;

public class KqClient : IClient
{
    protected SqlSugarScope sugarScope;
    internal Bot konatoBot;

    public TimeSpan ExpireTimeSpan = TimeSpan.FromMinutes(5);

    public event IClient.OnClientEventOccuredHandler<IClientEvent>? OnClientEventOccurred;

    public event Action<Exception>? OnStoppedUnexpectedly;

    public IUser Self => throw new NotImplementedException();

    public KqClient(Bot konatoBot)
    {
        sugarScope = new(new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=SaladimQBot.KonataMessages.db"
        });

        sugarScope.CodeFirst.InitTables<MessageStoraged>();
        this.konatoBot = konatoBot;
        konatoBot.OnGroupMessage += this.KonatoBot_OnGroupMessage;
    }

    private void KonatoBot_OnGroupMessage(Bot sender, GroupMessageEvent args)
    {

    }

    internal BotFriend

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
