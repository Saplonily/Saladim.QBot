﻿namespace SaladimQBot.Core;

/// <summary>
/// <para>Client类需要实现的接口</para>
/// </summary>
public interface IClient
{
    #region 获取消息
    /// <summary>
    /// 使用消息Id获取一个群消息
    /// </summary>
    /// <param name="messageId">消息id</param>
    /// <returns>消息实体</returns>
    IGroupMessage GetGroupMessageById(int messageId);

    /// <summary>
    /// 使用消息Id获取一个私聊消息
    /// </summary>
    /// <param name="messageId">消息id</param>
    /// <returns>消息实体</returns>
    IPrivateMessage GetPrivateMessageById(int messageId);

    /// <summary>
    /// 使用消息Id获取一个好友消息
    /// </summary>
    /// <param name="messageId">消息id</param>
    /// <returns>消息实体</returns>
    IFriendMessage GetFriendMessageById(int messageId);

    /// <summary>
    /// 使用消息Id获取一个消息, 应返回IMessage的子类
    /// </summary>
    /// <param name="messageId">消息id</param>
    /// <returns>消息实体</returns>
    IMessage GetMessageById(int messageId);
    #endregion

    #region 发送消息

    #region Private
    /// <summary>
    /// 使用消息实体发送私聊消息
    /// </summary>
    /// <param name="userId">对方qq号</param>
    /// <param name="groupId">作为临时会话时的来源群</param>
    /// <param name="messageEntity">消息实体</param>
    /// <returns>该消息实体</returns>
    Task<IPrivateMessage> SendPrivateMessageAsync(long userId, long? groupId, IMessageEntity messageEntity);

    /// <summary>
    /// 使用原始字符串发送私聊消息
    /// </summary>
    /// <param name="userId">对方qq号</param>
    /// <param name="groupId">作为临时会话时的来源群</param>
    /// <param name="rawString">消息实体</param>
    /// <returns>该消息实体</returns>
    Task<IPrivateMessage> SendPrivateMessageAsync(long userId, long? groupId, string rawString);

    #endregion

    #region Friend
    /// <summary>
    /// 使用消息实体发送好友消息
    /// </summary>
    /// <param name="friendUserId">对方qq号</param>
    /// <param name="messageEntity">消息实体</param>
    /// <returns>该消息实体</returns>
    Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, IMessageEntity messageEntity);

    /// <summary>
    /// 使用原始字符串发送好友消息
    /// </summary>
    /// <param name="friendUserId">对方qq号</param>
    /// <param name="rawString">消息实体</param>
    /// <returns>该消息实体</returns>
    Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, string rawString);
    #endregion

    #region Group
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
    #endregion

    #region Forward
    /// <summary>
    /// 向群里发送转发实体
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="forwardEntity"></param>
    /// <returns></returns>
    Task<IGroupMessage> SendGroupMessageAsync(long groupId, IForwardEntity forwardEntity);

    /// <summary>
    /// 向用户发送转发实体
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <param name="forwardEntity">转发实体</param>
    /// <returns></returns>
    Task<IPrivateMessage> SendPrivateMessageAsync(long userId, IForwardEntity forwardEntity);

    /// <summary>
    /// 向好友发送转发实体
    /// </summary>
    /// <param name="friendUserId">好友id</param>
    /// <param name="forwardEntity">转发实体</param>
    /// <returns></returns>
    Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, IForwardEntity forwardEntity);
    #endregion

    #region Reply

    Task<IPrivateMessage> ReplyMessageAsync(IPrivateMessage privateMessage, IMessageEntity msg);

    Task<IPrivateMessage> ReplyMessageAsync(IPrivateMessage privateMessage, string formattedString);

    Task<IGroupMessage> ReplyMessageAsync(IGroupMessage groupMessage, IMessageEntity msg);

    Task<IGroupMessage> ReplyMessageAsync(IGroupMessage groupMessage, string formattedString);

    #endregion

    #endregion

    #region 消息/成员处理
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
    /// 设置群名
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="newGroupName">新群号</param>
    Task SetGroupNameAsync(long groupId, string newGroupName);

    /// <summary>
    /// 设置群成员名片
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="userId">用户Id</param>
    /// <param name="newCard">新名片</param>
    /// <returns></returns>
    Task SetGroupCardAsync(long groupId, long userId, string newCard);

    #endregion

    #region 实体获取

    /// <summary>
    /// 获取一个群成员实体
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="userId">用户Id</param>
    IGroupUser GetGroupUser(long groupId, long userId);

    /// <summary>
    /// 获取一个QQ用户实体
    /// </summary>
    /// <param name="userId">用户Id</param>
    IUser GetUser(long userId);

    /// <summary>
    /// 获取一个群实体, 允许bot没加入
    /// </summary>
    /// <param name="groupId">群号</param>
    IGroup GetGroup(long groupId);

    /// <summary>
    /// 获取一个bot加入了的群
    /// </summary>
    /// <param name="groupId">群号</param>
    IJoinedGroup GetJoinedGroup(long groupId);

    /// <summary>
    /// 获取一个bot的好友
    /// </summary>
    /// <param name="userId">好友id</param>
    IFriendUser GetFriendUser(long friendUserId);

    #endregion

    /// <summary>
    /// Client发生事件Handler
    /// </summary>
    /// <typeparam name="TIClientEvent"></typeparam>
    /// <param name="clientEvent"></param>
    public delegate void OnClientEventOccuredHandler<in TIClientEvent>(TIClientEvent clientEvent) where TIClientEvent : IClientEvent;

    /// <summary>
    /// Client发生事件时引发
    /// </summary>
    event OnClientEventOccuredHandler<IClientEvent> OnClientEventOccurred;

    /// <summary>
    /// 出现异常停止时引发, 例如在Socket通讯时链接断开时伴随SocketException引发此事件
    /// </summary>
    event Action<Exception> OnStoppedUnexpectedly;

    /// <summary>
    /// bot号本身的用户实体
    /// </summary>
    IUser Self { get; }

    /// <summary>
    /// 创建一个MessageBuilder
    /// </summary>
    /// <returns></returns>
    IMessageEntityBuilder CreateMessageBuilder();

    /// <summary>
    /// 创建一个转发消息Builder
    /// </summary>
    /// <returns></returns>
    IForwardEntityBuilder CreateForwardBuilder();

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