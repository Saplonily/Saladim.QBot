namespace QBotDotnet.Core;

public interface IMessageReplyIdNode : IMessageEntityNode
{
    /// <summary>
    /// 消息id
    /// </summary>
    int MessageId { get; set; }
}