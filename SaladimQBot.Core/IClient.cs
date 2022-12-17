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

    #region 一堆用户层的事件

    #region 消息收到

    public delegate void OnMessageReceivedHandler<in TIMessage>(TIMessage message) where TIMessage : IMessage;
    public event OnMessageReceivedHandler<IMessage>? OnMessageReceived;

    public delegate void OnGroupMessageReceivedHandler<in TIGroupMessage, in TIJoinedGroup>(TIGroupMessage message, TIJoinedGroup group)
        where TIGroupMessage : IGroupMessage where TIJoinedGroup : IJoinedGroup;
    public event OnGroupMessageReceivedHandler<IGroupMessage, IJoinedGroup>? OnGroupMessageReceived;

    public delegate void OnPrivateMessageReceivedHandler<in TIPrivateMessage, in TIUser>(TIPrivateMessage message, TIUser user)
        where TIPrivateMessage : IPrivateMessage where TIUser : IUser;
    public event OnPrivateMessageReceivedHandler<IPrivateMessage, IUser>? OnPrivateMessageReceived;

    public delegate void OnFriendMessageReceivedHandler<in TIFriendMessage, in TIFriendUser>(TIFriendMessage message, TIFriendUser friendUser)
        where TIFriendMessage : IFriendMessage where TIFriendUser : IFriendUser;
    public event OnFriendMessageReceivedHandler<IFriendMessage, IFriendUser>? OnFriendMessageReceived;

    #endregion

    #region 消息撤回

    public delegate void OnMessageRecalledHandler<in TIMessage, in TIUser>(TIMessage message, TIUser operatorUser)
        where TIMessage : IMessage where TIUser : IUser;
    public event OnMessageRecalledHandler<IMessage, IUser>? OnMessageRecalled;

    public delegate void OnPrivateMessageRecalledHandler<in TIPrivateMessage, in TIUser>(TIPrivateMessage message, TIUser user)
        where TIPrivateMessage : IPrivateMessage where TIUser : IUser;
    public event OnPrivateMessageRecalledHandler<IPrivateMessage, IUser>? OnPrivateMessageRecalled;

    public delegate void OnGroupMessageRecalledHandler<in TIGroupMessage, in TIJoinedGroup, in TIGroupUser>
        (TIGroupMessage message, TIJoinedGroup group, TIGroupUser operatorUser)
        where TIGroupMessage : IGroupMessage where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupMessageRecalledHandler<IGroupMessage, IJoinedGroup, IGroupUser>? OnGroupMessageRecalled;

    #endregion

    #region Notice收到
    //好友添加
    public delegate void OnFriendAddedHandler<in TIFriendUser>(TIFriendUser user) where TIFriendUser : IFriendUser;
    public event OnFriendAddedHandler<IFriendUser>? OnFriendAdded;

    //群管理员变动
    public delegate void OnGroupAdminChangedHandler<in TIJoinedGroup, in TIGroupUser>(TIJoinedGroup group, TIGroupUser user, bool isSet)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupAdminChangedHandler<IJoinedGroup, IGroupUser>? OnGroupAdminChanged;
    public delegate void OnGroupAdminSetHandler<in TIJoinedGroup, in TIGroupUser>(TIJoinedGroup group, TIGroupUser user)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupAdminSetHandler<IJoinedGroup, IGroupUser>? OnGroupAdminSet;
    public delegate void OnGroupAdminCancelledHandler<in TIJoinedGroup, in TIGroupUser>(TIJoinedGroup group, TIGroupUser user)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupAdminCancelledHandler<IJoinedGroup, IGroupUser>? OnGroupAdminCancelled;

    //群精华变动
    public delegate void OnGroupEssenceSetHandler<in TIJoinedGroup, in TIGroupUser, in TIGroupMessage>
        (TIJoinedGroup group, TIGroupUser operatorUser, TIGroupUser user, TIGroupMessage message, bool isAdd)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser where TIGroupMessage : IGroupMessage;
    public event OnGroupEssenceSetHandler<IJoinedGroup, IGroupUser, IGroupMessage>? OnGroupEssenceSet;

    public delegate void OnGroupEssenceAddedHandler<in TIJoinedGroup, in TIGroupUser, in TIGroupMessage>
        (TIJoinedGroup group, TIGroupUser operatorUser, TIGroupUser user, TIGroupMessage message)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser where TIGroupMessage : IGroupMessage;
    public event OnGroupEssenceAddedHandler<IJoinedGroup, IGroupUser, IGroupMessage>? OnGroupEssenceAdded;

    public delegate void OnGroupEssenceRemovedHandler<in TIJoinedGroup, in TIGroupUser, in TIGroupMessage>
        (TIJoinedGroup group, TIGroupUser operatorUser, TIGroupUser user, TIGroupMessage message)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser where TIGroupMessage : IGroupMessage;
    public event OnGroupEssenceRemovedHandler<IJoinedGroup, IGroupUser, IGroupMessage>? OnGroupEssenceRemoved;

    //群文件上传
    public delegate void OnGroupFileUploadedHandler<in TIJoinedGroup, in TIGroupUser, in TIGroupFile>
        (TIJoinedGroup group, TIGroupUser uploader, TIGroupFile groupFile)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser where TIGroupFile : IGroupFile;
    public event OnGroupFileUploadedHandler<IJoinedGroup, IGroupUser, IGroupFile>? OnGroupFileUploaded;

    //离线文件收到
    public delegate void OnOfflineFileReceivedHandler<in TIUser, in TIOfflineFile>(TIUser user, TIOfflineFile file)
        where TIUser : IUser where TIOfflineFile : IOfflineFile;
    public event OnOfflineFileReceivedHandler<IUser, IOfflineFile>? OnOfflineFileReceived;

    //群禁言
    public delegate void OnGroupMemberBannedHandler<in TIJoinedGroup, in TIGroupUser>
        (TIJoinedGroup group, TIGroupUser groupUser, TIGroupUser operatorUser, TimeSpan timeSpan)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupMemberBannedHandler<IJoinedGroup, IGroupUser>? OnGroupMemberBanned;

    public delegate void OnGroupMemberBanLiftedHandler<in TIJoinedGroup, in TIGroupUser>
        (TIJoinedGroup group, TIGroupUser groupUser, TIGroupUser operatorUser)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupMemberBanLiftedHandler<IJoinedGroup, IGroupUser>? OnGroupMemberBanLifted;

    //special: 全员禁言
    public delegate void OnGroupAllUserBannedHandler<in TIJoinedGroup, in TIGroupUser>(TIJoinedGroup group, TIGroupUser operatorUser)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupAllUserBannedHandler<IJoinedGroup, IGroupUser>? OnGroupAllUserBanned;
    public delegate void OnGroupAllUserBanLiftedHandler<in TIJoinedGroup, in TIGroupUser>(TIJoinedGroup group, TIGroupUser operatorUser)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupAllUserBanLiftedHandler<IJoinedGroup, IGroupUser>? OnGroupAllUserBanLifted;

    //群名片变更
    public delegate void OnGroupMemberCardChangedHandler<in TIJoinedGroup, in TIGroupUser>
        (TIJoinedGroup group, TIGroupUser groupUser, string from, string to)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupMemberCardChangedHandler<IJoinedGroup, IGroupUser>? OnGroupMemberCardChanged;

    //群成员变更
    public delegate void OnGroupMemberChangedHandler<in TIJoinedGroup, in TIUser>(TIJoinedGroup group, TIUser user, bool isIncrease)
        where TIJoinedGroup : IJoinedGroup where TIUser : IUser;
    public event OnGroupMemberChangedHandler<IJoinedGroup, IUser>? OnGroupMemberChanged;

    public delegate void OnGroupMemberIncreasedHandler<in TIJoinedGroup, in TIGroupUser>(TIJoinedGroup group, TIGroupUser user)
        where TIJoinedGroup : IJoinedGroup where TIGroupUser : IGroupUser;
    public event OnGroupMemberIncreasedHandler<IJoinedGroup, IGroupUser>? OnGroupMemberIncreased;
    public delegate void OnGroupMemberDecreasedHandler<in TIJoinedGroup, in TIUser>(TIJoinedGroup group, TIUser user)
        where TIJoinedGroup : IJoinedGroup where TIUser : IUser;
    public event OnGroupMemberDecreasedHandler<IJoinedGroup, IUser>? OnGroupMemberDecreased;
    #endregion

    #region 请求

    //加好友请求
    public delegate void OnFriendAddRequestedHandler<in TIFriendAddRequest>(TIFriendAddRequest request)
        where TIFriendAddRequest : IFriendAddRequest;
    public event OnFriendAddRequestedHandler<IFriendAddRequest>? OnFriendAddRequested;
    public delegate void OnGroupJoinRequestedHandler<in TIGroupJoinRequest>(TIGroupJoinRequest request)
        where TIGroupJoinRequest : IGroupJoinRequest;
    public event OnGroupJoinRequestedHandler<IGroupJoinRequest>? OnGroupJoinRequested;
    public delegate void OnGroupInviteRequestedHandler<in TIGroupInviteRequest>(TIGroupInviteRequest request)
        where TIGroupInviteRequest : IGroupInviteRequest;
    public event OnGroupInviteRequestedHandler<IGroupInviteRequest>? OnGroupInviteRequested;


    #endregion

    #endregion

    /// <summary>
    /// bot号本身的实体
    /// </summary>
    IUser Self { get; }

    IMessageEntityBuilder CreateMessageBuilder();

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