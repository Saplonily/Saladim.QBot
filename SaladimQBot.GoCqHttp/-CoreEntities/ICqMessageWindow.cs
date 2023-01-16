using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public interface ICqMessageWindow : IMessageWindow
{
    /// <summary>
    /// 向消息窗口发送一个消息(使用消息实体)
    /// </summary>
    /// <param name="messageEntity">消息实体</param>
    Task<Message> SendMessageAsync(MessageEntity messageEntity);

    new Task<Message> SendMessageAsync(string rawString);

    Task<Message> SendMessageAsync(ForwardEntity forwardEntity);
}
