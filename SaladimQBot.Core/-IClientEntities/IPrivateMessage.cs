namespace SaladimQBot.Core;

public interface IPrivateMessage : IMessage
{
    MessageTempSource TempSource { get; }

    Task<IPrivateMessage> ReplyAsync(IMessageEntity msg);

    Task<IPrivateMessage> ReplyAsync(string rawString);
}
