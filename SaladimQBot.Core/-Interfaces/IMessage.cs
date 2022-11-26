namespace SaladimQBot.Core;

public interface IMessage : IClientEntity
{
    long MessageId { get; }

    IMessageEntity MessageEntity { get; }

    IUser Sender { get; }

    IMessageWindow MessageWindow { get; }
}