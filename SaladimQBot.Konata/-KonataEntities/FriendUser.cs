using System;
using System.Threading.Tasks;
using Konata.Core.Common;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.Konata;

public class FriendUser : User, IFriendUser
{
    internal const string DoesNotSupportMsg = "Konata does not support fetch infomation of FriendUser.";

    public override Sex Sex => throw new NotSupportedException(DoesNotSupportMsg);

    public override int Age => throw new NotSupportedException(DoesNotSupportMsg);

    public override int Level => throw new NotSupportedException(DoesNotSupportMsg);

    public override int LoginDays => throw new NotSupportedException(DoesNotSupportMsg);

    protected internal FriendUser(KqClient client, long userId, string nickname) : base(client, userId, nickname)
    {
    }

    public async override Task<IMessage> SendMessageAsync(IMessageEntity messageEntity)
        => await Client.SendFriendMessageAsync(UserId, messageEntity).ConfigureAwait(false);

    public async override Task<IMessage> SendMessageAsync(string rawString)
        => await Client.SendFriendMessageAsync(UserId, rawString).ConfigureAwait(false);

    public async override Task<IMessage> SendMessageAsync(IForwardEntity forwardEntity)
        => await Client.SendFriendMessageAsync(UserId, forwardEntity).ConfigureAwait(false);
}
