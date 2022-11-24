namespace SaladimQBot.Core;

/// <summary>
/// 接收图片时的Cq码
/// </summary>
public interface IMessageImageReceiveNode : IMessageEntityNode
{
    string File { get; }

    ImageSendType Type { get; }

    /// <summary>
    /// 图片子类型，在私聊消息上报时实现应为<see cref="ImageSendSubType.Invalid"/>
    /// </summary>
    ImageSendSubType SubType { get; }

    /// <summary>
    /// 若图片为秀图时的秀图特效id，非秀图应实现为<see cref="ImageShowType.Invalid"/>
    /// </summary>
    ImageShowType ShowType { get; }

    /// <summary>
    /// 转换成发送Node
    /// </summary>
    /// <returns></returns>
    IMessageImageSendNode ToSendNode();
}