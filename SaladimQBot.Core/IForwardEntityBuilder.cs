namespace SaladimQBot.Core;

/// <summary>
/// 转发消息Builder
/// </summary>
public interface IForwardEntityBuilder : IClientEntity
{
    /// <summary>
    /// 加入一条消息
    /// </summary>
    /// <param name="msg">具体消息实体</param>
    /// <returns>原本的消息转发Builder</returns>
    IForwardEntityBuilder AddMessage(IMessage msg);

    /// <summary>
    /// 加入一条自定义消息
    /// </summary>
    /// <param name="senderShowName">发送者显示昵称</param>
    /// <param name="sender">发送者实体</param>
    /// <param name="entity">消息实体</param>
    /// <param name="sendTime">显示的消息发送时间</param>
    /// <returns>原本的消息转发Builder</returns>
    IForwardEntityBuilder AddMessage(string senderShowName, IUser sender, IMessageEntity entity, DateTime sendTime);

    /// <summary>
    /// 加入一条自定义消息
    /// </summary>
    /// <param name="sender">发送者实体</param>
    /// <param name="entity">消息实体</param>
    /// <param name="sendTime">显示的消息发送时间</param>
    /// <returns>原本的消息转发Builder</returns>
    IForwardEntityBuilder AddMessage(IUser sender, IMessageEntity entity, DateTime sendTime);

    /// <summary>
    /// build, 并获取对应消息转发实体
    /// </summary>
    /// <returns></returns>
    IForwardEntity Build();
}
