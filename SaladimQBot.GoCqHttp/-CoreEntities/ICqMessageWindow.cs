using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public interface ICqMessageWindow : IMessageWindow
{
    Task<Message> SendMessageAsync(MessageEntity messageEntity);

    new Task<Message> SendMessageAsync(string rawString);
}
