namespace SaladimQBot.Core;

public interface IPrivateMessage : IMessage
{
    MessageTempSource TempSource { get; }

    long? TempSourceGroupId { get; }

    Task<IPrivateMessage> ReplyAsync(IMessageEntity msg);

    Task<IPrivateMessage> ReplyAsync(string rawString);
}
