namespace SaladimQBot.Core;

public interface IGroupMessage : IMessage
{
    new IGroupUser Sender { get; }

    IJoinedGroup Group { get; }

    Task<IGroupMessage> ReplyAsync(IMessageEntity msg);

    Task<IGroupMessage> ReplyAsync(string rawString);
}