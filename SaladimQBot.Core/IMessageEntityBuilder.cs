namespace SaladimQBot.Core;

/// <summary>
/// 消息Builder, 使用<see cref="IClient.CreateMessageBuilder"/>来获取
/// </summary>
public interface IMessageEntityBuilder : IClientEntity
{
    /// <summary>
    /// 添加一段文字
    /// </summary>
    /// <param name="text">具体文字</param>
    /// <returns>builder, 用于链式调用</returns>
    IMessageEntityBuilder WithText(string text);

    /// <summary>
    /// 添加一张图片
    /// </summary>
    /// <param name="uri">图片uri, 只确保允许http https file协议</param>
    /// <returns>builder, 用于链式调用</returns>
    IMessageEntityBuilder WithImage(Uri uri);

    /// <summary>
    /// 添加一个at
    /// </summary>
    /// <param name="userId">被at者id</param>
    /// <returns>builder, 用于链式调用</returns>
    IMessageEntityBuilder WithAt(long userId);

    /// <summary>
    /// 添加一个at
    /// </summary>
    /// <param name="userId">被at者id</param>
    /// <param name="nameWhenUserNotExists">当被at者不在群内时替代使用的名字</param>
    /// <returns>builder, 用于链式调用</returns>
    IMessageEntityBuilder WithAt(long userId, string nameWhenUserNotExists);

    /// <summary>
    /// 添加一个表情
    /// </summary>
    /// <param name="faceId">表情id</param>
    /// <returns>builder, 用于链式调用</returns>
    IMessageEntityBuilder WithFace(int faceId);

    /// <summary>
    /// 添加一个对消息的回复
    /// </summary>
    /// <param name="message">消息实体</param>
    /// <returns>builder, 用于链式调用</returns>
    IMessageEntityBuilder WithReply(IMessage message);

    /// <summary>
    /// 添加一个自定义节点, 用于对框架未实现但协议已实现的节点
    /// </summary>
    /// <param name="name">节点昵称</param>
    /// <param name="args">节点参数</param>
    /// <returns>builder, 用于链式调用</returns>
    IMessageEntityBuilder WithUnImpl(string name, IDictionary<string, string> args);

    /// <summary>
    /// Build
    /// </summary>
    /// <param name="prepareRawString">是否在构建的同时缓存rawString格式化结果, 否则之后在初次获取时才进行格式化</param>
    /// <returns>对应消息实体</returns>
    IMessageEntity Build(bool prepareRawString = false);
}
