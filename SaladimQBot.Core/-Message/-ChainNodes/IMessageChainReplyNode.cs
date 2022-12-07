namespace SaladimQBot.Core;

public interface IMessageChainReplyNode : IMessageChainNode
{
    /// <summary>
    /// 对应消息
    /// </summary>
    IMessage MessageBeReplied { get; set; }
}