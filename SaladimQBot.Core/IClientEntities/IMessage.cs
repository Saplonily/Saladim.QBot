namespace SaladimQBot.Core;

public interface IMessage : IClientEntity
{
    int MessageId { get; }

    IMessageEntity MessageEntity { get; }

    IMessageWindow MessageWindow { get; }

    IUser Sender { get; }

    DateTime SendTime { get; }
}