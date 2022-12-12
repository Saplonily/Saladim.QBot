namespace SaladimQBot.Core;

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
    /// 向群里发送转发实体
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="forwardEntity"></param>
    /// <returns></returns>
    Task<IGroupMessage> SendGroupMessageAsync(long groupId, IForwardEntity forwardEntity);

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
    /// bot号本身的实体
    /// </summary>
    IUser Self { get; }

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