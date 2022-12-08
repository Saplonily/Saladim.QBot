namespace SaladimQBot.Core;

public interface IForwardContentNode : IForwardNode
{
    string SenderShowName { get; }

    IUser Sender { get; }

    IMessageEntity MessageEntity { get; }

    DateTime SendTime { get; }
}
