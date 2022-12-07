namespace SaladimQBot.Core;

public interface IMessageChainImageNode : IMessageChainNode
{
    /// <summary>
    /// 实现为图片url或者本地文件
    /// </summary>
    string FileUri { get; }
}
