namespace SaladimQBot.Core;

public interface IMessageChainImageNode : IMessageChainNode
{
    /// <summary>
    /// 图片所在uri, 一般协议会为http/https或本地
    /// </summary>
    Uri FileUri { get; }
}
