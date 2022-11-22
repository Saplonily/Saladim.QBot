namespace SaladimQBot.Core;

public interface IMessage
{
    long MessageId { get; }

    IMessageEntity MessageEntity { get; }

    IUser Sender { get; }
}