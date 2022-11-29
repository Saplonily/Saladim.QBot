﻿namespace SaladimQBot.Core;

/// <summary>
/// <para>Client类需要实现的接口</para>
/// </summary>
public interface IClient
{
    /// <summary>
    /// 使用消息Id获取一个群消息
    /// </summary>
    /// <param name="messageId">消息id</param>
    /// <returns>消息实体</returns>
    IGroupMessage GetGroupMessageById(int messageId);

    /// <summary>
    /// 使用消息Id获取一个私聊消息
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    IPrivateMessage GetPrivateMessageById(int messageId);

    /// <summary>
    /// 使用消息实体发送私聊消息
    /// </summary>
    /// <param name="userId">对方qq号</param>
    /// <param name="messageEntity">消息实体</param>
    /// <returns>该消息实体</returns>
    Task<IPrivateMessage> SendPrivateMessageAsync(long userId, IMessageEntity messageEntity);

    /// <summary>
    /// 使用原始字符串发送私聊消息
    /// </summary>
    /// <param name="userId">对方qq号</param>
    /// <param name="rawString">消息实体</param>
    /// <returns>该消息实体</returns>
    Task<IPrivateMessage> SendPrivateMessageAsync(long userId, string rawString);

    /// <summary>
    /// 使用消息实体发送群消息
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="messageEntity">消息实体</param>
    /// <returns>该消息实体</returns>
    Task<IGroupMessage> SendGroupMessageAsync(long groupId, IMessageEntity messageEntity);

    /// <summary>
    /// 使用原始字符串发送群消息
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="messageEntity">消息实体</param>
    /// <returns>该消息实体</returns>
    Task<IGroupMessage> SendGroupMessageAsync(long groupId, string rawString);

    /// <summary>
    /// 撤回一条消息
    /// </summary>
    /// <param name="messageId">消息id</param>
    Task RecallMessageAsync(int messageId);

    /// <summary>
    /// 禁言一个群员
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="userId">用户Id</param>
    /// <param name="time">时间</param>
    Task BanGroupUserAsync(long groupId, long userId, TimeSpan time);

    /// <summary>
    /// 解禁一个群员
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="userId">用户Id</param>
    Task LiftBanGroupUserAsync(long groupId, long userId);

    /// <summary>
    /// 获取一个群成员实体
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="userId">用户Id</param>
    IGroupUser GetGroupUser(long groupId, long userId);

    /// <summary>
    /// 开始该Client的连接
    /// </summary>
    /// <returns>状态值</returns>
    Task StartAsync();

    /// <summary>
    /// 停止该Client的连接
    /// </summary>
    /// <returns>状态值</returns>
    Task StopAsync();
}