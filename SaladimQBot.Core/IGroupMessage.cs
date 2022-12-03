namespace SaladimQBot.Core;

public interface IGroupMessage : IMessage
{
    new IGroupUser Sender { get; }

    IJoinedGroup Group { get; }
}