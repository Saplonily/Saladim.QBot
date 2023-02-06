using System;
using System.Threading.Tasks;
using SaladimQBot.Core;
using SqlSugar;

namespace SaladimQBot.Konata;

public class KqClient : IClient
{
    protected SqlSugarScope sugarScope;

    public KqClient()
    {
        sugarScope = new(new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = "SaladimQBot.Konata.MessageDb.db"
        });
    }

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

    public event IClient.OnClientEventOccuredHandler<IClientEvent>? OnClientEventOccurred;

    public event Action<Exception>? OnStoppedUnexpectedly;

    public IUser Self { get; }

    public IMessageEntityBuilder CreateMessageBuilder()
    {
        throw new NotImplementedException();
    }

    public IForwardEntityBuilder CreateForwardBuilder()
    {
        throw new NotImplementedException();
    }

    public Task StartAsync()
    {
        throw new NotImplementedException();
    }

    public Task StopAsync()
    {
        throw new NotImplementedException();
    }
}
