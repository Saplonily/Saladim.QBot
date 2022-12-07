namespace SaladimQBot.Core;

public interface IForwardCustomNode : IForwardNode
{
    string SenderShowName { get; }

    IUser Sender { get; }

    IMessageEntity MessageEntity { get; }
}
