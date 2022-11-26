namespace SaladimQBot.Core;

public interface IMessageWindow : IClientEntity
{
    /// <summary>
    /// 使用消息实体发送消息
    /// </summary>
    /// <param name="messageEntity">消息实体</param>
    /// <returns>该消息</returns>
    Task<IMessage> SendMessageAsync(IMessageEntity messageEntity);

    /// <summary>
    /// 使用原始字符串发送消息
    /// </summary>
    /// <param name="rawString">消息原始字符串</param>
    /// <returns>该消息</returns>
    Task<IMessage> SendMessageAsync(string rawString);
}
