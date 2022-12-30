using System.Diagnostics;
using System.Text.Json;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("CqClient, Started={Started}, StartedBefore={StartedBefore}")]
public abstract class CqClient : IClient
{
    public abstract ICqSession ApiSession { get; }

    public abstract ICqSession PostSession { get; }

    public abstract TimeSpan ExpireTimeSpan { get; }

    //TODO 改为SelfUser类型
    /// <summary>
    /// Client自身用户
    /// </summary>
    public User Self => lazySelf.Value;

    internal Lazy<User> lazySelf;

    /// <summary>
    /// 该Client之前是否尝试开启过
    /// </summary>
    public bool StartedBefore { get; protected set; }

    /// <summary>
    /// 该Client是否开启
    /// </summary>
    public bool Started { get; protected set; }

    protected Logger logger;
    protected readonly Dictionary<CqApi, IIndependentExpirable<CqApiCallResultData>> cachedApiCallResultData = new();

    public CqClient(LogLevel logLevelLimit)
    {
        OnPost += InternalPostProcessor;
        logger =
            new LoggerBuilder()
                .WithLevelLimit(logLevelLimit)
                .WithAction(s => OnLog?.Invoke(s))
                .WithFormatter(ClientLogFormatter)
                .Build();
        lazySelf = new(LazySelfFactory, true);

        static string ClientLogFormatter(LogLevel l, string s, string? ss, string content)
            => $"[" +
            $"{l}][{s}/{(ss is null ? "" : $"{ss}")}" +
            $"] {content}";
    }

    private User LazySelfFactory()
    {
        GetLoginAction a = new();
        var (_, d) = this.CallApiImplicitlyWithCheckingAsync<GetLoginActionResultData>(a).Result;
        return this.GetUser(d.UserId);

    }

    #region OnPost和OnLog事件

    public delegate void OnPostHandler(CqPost post);
    /// <summary>
    /// <para>收到原始上报时发生,CqPost类型参数为实际实体上报类</para>
    /// <para>事件源以「同步」方式触发此事件</para>
    /// </summary>
    public event OnPostHandler OnPost;

    public delegate void OnLogHandler(string logMessageString);
    /// <summary>
    /// <para>客户端日志事件</para>
    /// </summary>
    public event OnLogHandler? OnLog;

    #endregion

    #region CallApi

    /// <summary>
    /// 使用该客户端调用原始Api
    /// </summary>
    /// <param name="api">要调用的api实体</param>
    /// <returns>一个结果为<see cref="CqApiCallResult"/>的task</returns>
    public async Task<CqApiCallResult?> CallApiAsync(CqApi api)
    {
        if (!Started)
            throw new ClientException(this, ClientException.ExceptionType.NotStartedBeforeCallApi);
        if (logger.NeedLogging(LogLevel.Debug))
            logger.LogDebug(
                "Client", "ApiCall", $"Ready for api '{api.ApiName}' call: " +
                $"{CqApiJsonSerializer.SerializeApiParamsToNode(api).ToJsonString(CqJsonOptions.Instance)}"
                );
        return await ApiSession.CallApiAsync(api).ConfigureAwait(false);
    }

    /// <summary>
    /// 使用该客户端调用原始Api, 同时转换返回的Data类型
    /// </summary>
    /// <typeparam name="T">api调用结果的Data(<see cref="CqApiCallResult.Data"/>)的类型</typeparam>
    /// <param name="api">要调用的api实体</param>
    /// <returns></returns>
    public async Task<(CqApiCallResult?, T?)> CallApiAsync<T>(CqApi api) where T : CqApiCallResultData
    {
        var result = await CallApiAsync(api).ConfigureAwait(false);
        return (result, result?.Data as T);
    }

    #endregion

    #region Start & Stops 实现

    /// <summary>
    /// 开启客户端
    /// </summary>
    /// <returns>异步Task</returns>
    /// <exception cref="ClientException"></exception>
    public Task StartAsync()
    {
        if (Started)
            throw new ClientException(this, ClientException.ExceptionType.AlreadyStarted);
        if (ApiSession.Started || PostSession.Started)
        {
            logger.LogWarn("Client", "Connection", "Either of session has been started.");
        }
        return InternalStartAsync();
    }

    /// <summary>
    /// 关闭客户端
    /// </summary>
    /// <returns>异步Task</returns>
    /// <exception cref="ClientException"></exception>
    public Task StopAsync()
    {
        if (!StartedBefore)
            throw new ClientException(this, ClientException.ExceptionType.NotStartedBefore);
        if (!Started)
            throw new ClientException(this, ClientException.ExceptionType.AlreadyStopped);
        InternalStop();
        return Task.CompletedTask;
    }

    #endregion

    #region Internal Start & Stops 实现

    internal async Task InternalStartAsync()
    {
        try
        {
            //我觉得会有人会换bot号所以我们需要每次都刷新一遍自己(
            lazySelf = new(LazySelfFactory, true);

            logger.LogInfo("Client", "Connection", "Connecting api session...");
            await ApiSession.StartAsync().ConfigureAwait(false);
            logger.LogInfo("Client", "Connection", "Connecting post session...");
            await PostSession.StartAsync().ConfigureAwait(false);
            logger.LogInfo("Client", "Connection", "Connection completed.");

            PostSession.OnReceived += OnSessionReceived;

            StartedBefore = true;
            Started = true;
        }
        catch (Exception ex)
        {
            this.InternalStop();

            Started = false;
            var msg = $"Internal session error, please check the inner exceptions. Trying to stop the sessions and client.";
            var clientException = new ClientException(
                this,
                ClientException.ExceptionType.SessionInternal,
                innerException: ex,
                message: msg
                );
            logger.LogWarn("Client", "Connection", $"Connection failed. Please check the thrown Exception.");
            throw clientException;
        }
        return;
    }

    internal void InternalStop()
    {
        if (Started)
        {
            logger.LogInfo("Client", "Connection", "Stopping connection...");
            ApiSession.Dispose();
            PostSession.Dispose();
            PostSession.OnReceived -= OnSessionReceived;
            Started = false;
        }
        else
        {
            logger.LogWarn("Client", "Connection", "Try to stop a not started client.");
        }
        return;
    }

    internal void OnSessionReceived(in JsonDocument srcDoc)
    {
        //TODO 可选的将所有switch分支抽离为一个函数
        CqJsonPostLoader loader = new(srcDoc.RootElement);
        CqPostType postType = loader.EnumFromString<CqPostType>(StringConsts.PostTypeProperty);
        switch (postType)
        {
            case CqPostType.MessageSent:
            case CqPostType.Message:
            {
                var subType = loader.EnumFromString<CqMessageSubType>(StringConsts.MessagePostSubTypeProperty);
                var targetType = CqTypeMapper.FindClassForCqMessagePostType(subType);
                if (targetType is null)
                {
                    if (logger.NeedLogging(LogLevel.Warn))
                        logger.LogWarn("Client", "PostParsing", $"Not found target type for {subType}");
                    return;
                }
                CqMessagePost? messagePost =
                    JsonSerializer.Deserialize(srcDoc, targetType, CqJsonOptions.Instance).AsCast<CqMessagePost>();
                if (messagePost is null)
                {
                    if (logger.NeedLogging(LogLevel.Warn))
                        logger.LogWarn("Client", "PostParsing", "Failed to deserialize a document to a CqMessagePost.");
                    return;
                }
                OnPost(messagePost);
            }
            break;
            case CqPostType.Notice:
            {
                var subType = loader.EnumFromString<CqNoticeType>(StringConsts.NoticeTypeProperty);
                if (subType == CqNoticeType.SystemNotice)
                {
                    var notifyType = loader.EnumFromString<CqNotifySubType>(StringConsts.NotifySubTypeProperty);
                    var targetType = CqTypeMapper.FindClassForCqNotifyNoticePostType(notifyType);
                    if (targetType is null)
                    {
                        if (logger.NeedLogging(LogLevel.Warn))
                            logger.LogWarn("Client", "PostParsing", "Not found targetType for CqNotifyNoticePost.");
                        return;
                    }
                    CqNotifyNoticePost? cqNotifyNoticePost =
                        JsonSerializer.Deserialize(srcDoc, targetType, CqJsonOptions.Instance).AsCast<CqNotifyNoticePost>();
                    if (cqNotifyNoticePost is null)
                    {
                        if (logger.NeedLogging(LogLevel.Warn))
                            logger.LogWarn("Client", "PostParsing", "Failed to deserialize a document to a CqNotifyNoticePost.");
                        return;
                    }
                    OnPost(cqNotifyNoticePost);
                }
                else if (subType != CqNoticeType.Invalid)
                {
                    var noticeType = loader.EnumFromString<CqNoticeType>(StringConsts.NoticeTypeProperty);
                    var targetType = CqTypeMapper.FindClassForCqNoticeType(noticeType);
                    if (targetType is null)
                    {
                        if (logger.NeedLogging(LogLevel.Warn))
                            logger.LogWarn("Client", "PostParsing", "Not found targetType for CqNoticePost.");
                        return;
                    }
                    CqNoticePost? cqNoticePost =
                        JsonSerializer.Deserialize(srcDoc, targetType, CqJsonOptions.Instance).AsCast<CqNoticePost>();
                    if (cqNoticePost is null)
                    {
                        if (logger.NeedLogging(LogLevel.Warn))
                            logger.LogWarn("Client", "PostParsing", "Failed to deserialize a document to a CqNoticePost.");
                        return;
                    }
                    OnPost(cqNoticePost);
                }
                else
                {
                    if (logger.NeedLogging(LogLevel.Warn))
                        logger.LogWarn("Client", "PostParsing", "Invalid CqNoticeType.");
                    return;
                }

            }
            break;
            case CqPostType.Request:
            {
                var subType = loader.EnumFromString<CqRequestType>(StringConsts.RequestTypeProperty);
                var targetType = CqTypeMapper.FindClassForCqRequestPostType(subType);

                if (targetType is null)
                {
                    if (logger.NeedLogging(LogLevel.Warn))
                        logger.LogWarn("Client", "PostParsing", "Not found targetType for CqRequestType.");
                    return;
                }
                CqRequestPost? requestPost =
                    JsonSerializer.Deserialize(srcDoc, targetType, CqJsonOptions.Instance)?.Cast<CqRequestPost>();
                if (requestPost is null)
                {
                    if (logger.NeedLogging(LogLevel.Warn))
                        logger.LogWarn("Client", "PostParsing", "Failed to deserialize a document to a CqRequestPost.");
                    return;
                }
                OnPost(requestPost);
            }
            break;
            case CqPostType.MetaEvent:
                //TODO MetaEvent支持
                break;
        }

    }

    #endregion

    #region IExpirable<T> 和 DependencyExpirable Maker

    internal SourceExpirable<TResultData> MakeExpirableApiCallResultData<TResultData>(CqApi api) where TResultData : CqApiCallResultData
    {
        IndependentExpirable<TResultData> ex;
        bool cached = cachedApiCallResultData.TryGetValue(api, out var exFounded);
        var exFound = exFounded?.Cast<IndependentExpirable<TResultData>>();
        if (cached)
        {
            ex = exFound!;
        }
        else
        {
            ex = new IndependentExpirable<TResultData>(ApiCallResultDataFactory, this.ExpireTimeSpan);
            if (!cached)
            {
                //TODO: 定期删除过期很久的值缓存
                cachedApiCallResultData.Add(api, ex);
            }
            else
            {
                cachedApiCallResultData[api] = ex;
            }
        }
        return new(ex);
        TResultData ApiCallResultDataFactory()
            => this.CallApiImplicitlyWithCheckingAsync(api).Result.Data!.Cast<TResultData>();

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<挂起>")]
    internal IDependencyExpirable<TChild> MakeDependencyExpirable<TChild, TSource>(
        IDependencyExpirable<TSource> source,
        Func<TSource, TChild> valueGetter)
        where TChild : notnull where TSource : class
            => DependencyExpirable<TChild>.Create(source, valueGetter);

    internal IDependencyExpirable<TChild> MakeDependencyExpirable<TChild, TSource>(
        IDependencyExpirable<TSource> source,
        TChild presetValue,
        Func<TSource, TChild> valueGetter)
        where TChild : notnull where TSource : class
            => DependencyExpirable<TChild>.Create(presetValue, ExpireTimeSpan, source, valueGetter);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<挂起>")]
    internal IExpirable<T> MakeNoneExpirableExpirable<T>(Func<T> valueFactory) where T : notnull
        => new NonExpirableExpirable<T>(valueFactory);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<挂起>")]
    internal IExpirable<T> MakeNoneExpirableExpirable<T>(T value) where T : notnull
        => new NonExpirableExpirable<T>(value);

    #endregion

    internal void InternalPostProcessor(CqPost post)
    {
        switch (post)
        {
            case CqMessagePost messagePost:
                switch (messagePost)
                {
                    case CqGroupMessagePost groupMessagePost:
                    {
                        if (groupMessagePost.AnonymousSender is not null)
                        {
                            logger.LogInfo("Client", "PostProcessor", "Anonymous message received. But not supported for now.");
                            return;
                        }
                        GroupMessage gm = GroupMessage.CreateFromGroupMessagePost(this, groupMessagePost);
                        OnMessageReceived?.Invoke(gm);
                        OnGroupMessageReceived?.Invoke(gm, gm.Group);
                    }
                    break;

                    case CqPrivateMessagePost privateMessagePost:
                    {
                        if (privateMessagePost.TempSource is MessageTempSource.Invalid)
                        {
                            FriendMessage fm = FriendMessage.CreateFromPrivateMessagePost(this, privateMessagePost);
                            OnMessageReceived?.Invoke(fm);
                            OnPrivateMessageReceived?.Invoke(fm, fm.Sender);
                            OnFriendMessageReceived?.Invoke(fm, fm.Sender);
                            //ftm (无端联想)
                            //我sll不是跨啊(
                        }
                        else
                        {
                            PrivateMessage pm = PrivateMessage.CreateFromPrivateMessagePost(this, privateMessagePost);
                            OnMessageReceived?.Invoke(pm);
                            OnPrivateMessageReceived?.Invoke(pm, pm.Sender);
                        }
                    }
                    break;
                }
                break;

            case CqNoticePost noticePost:
                switch (noticePost)
                {
                    case CqFriendMessageRecalledNoticePost notice:
                    {
                        var privateMsg = this.GetPrivateMessageById(notice.MessageId);
                        OnMessageRecalled?.Invoke(privateMsg, privateMsg.Sender);
                        OnPrivateMessageRecalled?.Invoke(privateMsg, privateMsg.Sender);
                    }
                    break;

                    case CqGroupMessageRecalledNoticePost notice:
                    {
                        var groupMsg = this.GetGroupMessageById(notice.MessageId);
                        var operatorUser = this.GetGroupUser(notice.GroupId, notice.UserId);
                        OnMessageRecalled?.Invoke(groupMsg, operatorUser);
                        OnGroupMessageRecalled?.Invoke(groupMsg, groupMsg.Group, operatorUser);
                    }
                    break;

                    case CqFriendAddedNoticePost notice:
                    {
                        OnFriendAdded?.Invoke(this.GetFriendUser(notice.UserId));
                    }
                    break;

                    case CqGroupAdminChangedNoticePost notice:
                    {
                        bool isSet = notice.SubType == CqGroupAdminChangedNoticePost.NoticeSubType.Set;
                        bool isCancel = notice.SubType == CqGroupAdminChangedNoticePost.NoticeSubType.Cancel;
                        if (isSet != !isCancel)
                            throw new ClientException(
                                this,
                                ClientException.ExceptionType.EntityCreationFailed,
                                "GroupAdminChangedNoticePost got a none set none cancel SubType."
                                );
                        JoinedGroup group = this.GetJoinedGroup(notice.GroupId);
                        GroupUser user = this.GetGroupUser(group, notice.UserId);
                        OnGroupAdminChanged?.Invoke(group, user, isSet);
                        if (isSet)
                            OnGroupAdminSet?.Invoke(group, user);
                        else
                            OnGroupAdminCancelled?.Invoke(group, user);
                    }
                    break;

                    case CqGroupEssenceSetNoticePost notice:
                    {
                        bool isAdd = notice.SubType == CqGroupEssenceSetNoticePost.NoticeSubType.Add;
                        bool isDelete = notice.SubType == CqGroupEssenceSetNoticePost.NoticeSubType.Delete;
                        if (isAdd != !isDelete)
                            throw new ClientException(
                                this,
                                ClientException.ExceptionType.EntityCreationFailed,
                                "GroupEssenceSetNoticePost got a none add none delete SubType."
                                );
                        JoinedGroup group = this.GetJoinedGroup(notice.GroupId);
                        GroupUser user = this.GetGroupUser(group, notice.SenderId);
                        GroupUser operatorUser = this.GetGroupUser(group, notice.OperatorId);
                        GroupMessage message = this.GetGroupMessageById(notice.MessageId);
                        OnGroupEssenceSet?.Invoke(group, operatorUser, user, message, isAdd);
                        if (isAdd)
                            OnGroupEssenceAdded?.Invoke(group, operatorUser, user, message);
                        else
                            OnGroupEssenceRemoved?.Invoke(group, operatorUser, user, message);
                    }
                    break;

                    case CqGroupFileUploadedNoticePost notice:
                    {
                        GroupUser uploader = this.GetGroupUser(notice.GroupId, notice.UserId);
                        JoinedGroup group = this.GetJoinedGroup(notice.GroupId);
                        GroupFile groupFile = new(this, notice.File);
                        OnGroupFileUploaded?.Invoke(group, uploader, groupFile);
                    }
                    break;

                    case CqGroupMemberBannedNoticePost notice:
                    {
                        bool isBan = notice.SubType == CqGroupMemberBannedNoticePost.NoticeSubType.Ban;
                        bool isLiftBan = notice.SubType == CqGroupMemberBannedNoticePost.NoticeSubType.LiftBan;
                        if (isBan != !isLiftBan)
                            throw new ClientException(
                                this,
                                ClientException.ExceptionType.EntityCreationFailed,
                                "GroupMemberBannedNoticePost got a none Ban none LiftBan SubType."
                                );
                        GroupUser operatorUser = this.GetGroupUser(notice.GroupId, notice.OperatorId);
                        JoinedGroup group = this.GetJoinedGroup(notice.GroupId);
                        //确保不是全员禁言
                        if (notice.UserId != 0)
                        {
                            GroupUser user = this.GetGroupUser(notice.GroupId, notice.UserId);
                            if (isBan)
                                OnGroupMemberBanned?.Invoke(group, user, operatorUser, TimeSpan.FromSeconds(notice.Duration));
                            else
                                OnGroupMemberBanLifted?.Invoke(group, user, operatorUser);
                        }
                        else
                        {
                            if (isBan)
                                OnGroupAllUserBanned?.Invoke(group, operatorUser);
                            else
                                OnGroupAllUserBanLifted?.Invoke(group, operatorUser);
                        }
                    }
                    break;

                    case CqGroupMemberCardChangedNoticePost notice:
                    {
                        GroupUser user = this.GetGroupUser(notice.GroupId, notice.UserId);
                        JoinedGroup group = this.GetJoinedGroup(notice.GroupId);
                        OnGroupMemberCardChanged?.Invoke(group, user, notice.CardOld, notice.CardNew);
                    }
                    break;

                    case CqGroupMemberDecreaseNoticePost notice:
                    {
                        User user = this.GetUser(notice.UserId);
                        JoinedGroup group = this.GetJoinedGroup(notice.GroupId);
                        OnGroupMemberChanged?.Invoke(group, user, false);
                        OnGroupMemberDecreased?.Invoke(group, user);
                    }
                    break;

                    case CqGroupMemberIncreaseNoticePost notice:
                    {
                        GroupUser user = this.GetGroupUser(notice.GroupId, notice.UserId);
                        JoinedGroup group = this.GetJoinedGroup(notice.GroupId);
                        OnGroupMemberChanged?.Invoke(group, user, true);
                        OnGroupMemberIncreased?.Invoke(group, user);
                    }
                    break;

                    case CqOfflineFileUploadedNoticePost notice:
                    {
                        User user = this.GetUser(notice.UserId);
                        OfflineFile offlineFile = new(this, notice.File);
                        OnOfflineFileReceived?.Invoke(user, offlineFile);
                    }
                    break;
                }
                break;

            case CqRequestPost requestPost:
                switch (requestPost)
                {
                    case CqFriendRequestPost request:
                    {
                        var r = FriendAddRequest.CreateFromPost(this, request);
                        OnFriendAddRequested?.Invoke(r);
                    }
                    break;
                    case CqGroupRequestPost request:
                    {
                        if (request.SubType is CqGroupRequestPost.RequestSubType.Add)
                        {
                            var r = GroupJoinRequest.CreateFromPost(this, request);
                            OnGroupJoinRequested?.Invoke(r);
                        }
                        else if (request.SubType is CqGroupRequestPost.RequestSubType.Invite)
                        {
                            var r = GroupInviteRequest.CreateFromPost(this, request);
                            OnGroupInviteRequested?.Invoke(r);
                        }
                    }
                    break;
                }
                break;
        }
    }

    #region 一堆用户层的事件

    #region 消息收到
    public event IClient.OnMessageReceivedHandler<Message>? OnMessageReceived;
    public event IClient.OnGroupMessageReceivedHandler<GroupMessage, JoinedGroup>? OnGroupMessageReceived;
    public event IClient.OnPrivateMessageReceivedHandler<PrivateMessage, User>? OnPrivateMessageReceived;
    public event IClient.OnFriendMessageReceivedHandler<FriendMessage, FriendUser>? OnFriendMessageReceived;
    #endregion

    #region 消息撤回
    public event IClient.OnMessageRecalledHandler<Message, User>? OnMessageRecalled;
    public event IClient.OnPrivateMessageRecalledHandler<PrivateMessage, User>? OnPrivateMessageRecalled;
    public event IClient.OnGroupMessageRecalledHandler<GroupMessage, JoinedGroup, GroupUser>? OnGroupMessageRecalled;
    #endregion

    #region Notice收到
    //好友添加
    public event IClient.OnFriendAddedHandler<FriendUser>? OnFriendAdded;

    //群管理员变动
    public event IClient.OnGroupAdminChangedHandler<JoinedGroup, GroupUser>? OnGroupAdminChanged;
    public event IClient.OnGroupAdminSetHandler<JoinedGroup, GroupUser>? OnGroupAdminSet;
    public event IClient.OnGroupAdminCancelledHandler<JoinedGroup, GroupUser>? OnGroupAdminCancelled;

    //群精华变动
    public event IClient.OnGroupEssenceSetHandler<JoinedGroup, GroupUser, GroupMessage>? OnGroupEssenceSet;
    public event IClient.OnGroupEssenceAddedHandler<JoinedGroup, GroupUser, GroupMessage>? OnGroupEssenceAdded;
    public event IClient.OnGroupEssenceRemovedHandler<JoinedGroup, GroupUser, GroupMessage>? OnGroupEssenceRemoved;

    //群文件上传
    public event IClient.OnGroupFileUploadedHandler<JoinedGroup, GroupUser, GroupFile>? OnGroupFileUploaded;

    //群禁言
    public event IClient.OnGroupMemberBannedHandler<JoinedGroup, GroupUser>? OnGroupMemberBanned;
    public event IClient.OnGroupMemberBanLiftedHandler<JoinedGroup, GroupUser>? OnGroupMemberBanLifted;
    //special: 全员禁言
    public event IClient.OnGroupAllUserBannedHandler<JoinedGroup, GroupUser>? OnGroupAllUserBanned;
    public event IClient.OnGroupAllUserBanLiftedHandler<JoinedGroup, GroupUser>? OnGroupAllUserBanLifted;

    //群名片变更
    public event IClient.OnGroupMemberCardChangedHandler<JoinedGroup, GroupUser>? OnGroupMemberCardChanged;

    //离线文件收到
    public event IClient.OnOfflineFileReceivedHandler<User, OfflineFile>? OnOfflineFileReceived;

    //群成员变更
    public event IClient.OnGroupMemberChangedHandler<JoinedGroup, User>? OnGroupMemberChanged;
    public event IClient.OnGroupMemberIncreasedHandler<JoinedGroup, GroupUser>? OnGroupMemberIncreased;
    public event IClient.OnGroupMemberDecreasedHandler<JoinedGroup, User>? OnGroupMemberDecreased;
    #endregion

    #region 请求

    //加好友请求
    public event IClient.OnFriendAddRequestedHandler<FriendAddRequest>? OnFriendAddRequested;
    public event IClient.OnGroupJoinRequestedHandler<GroupJoinRequest>? OnGroupJoinRequested;
    public event IClient.OnGroupInviteRequestedHandler<GroupInviteRequest>? OnGroupInviteRequested;

    #endregion

    #endregion

    #region 实用方法
    public MessageEntityBuilder CreateMessageBuilder()
        => new(this);

    public MessageEntityBuilder CreateMessageBuilder(Message msgToReply)
        => new MessageEntityBuilder(this).WithReply(msgToReply);

    public MessageEntityBuilder CreateMessageBuilderWithText(string text)
        => new MessageEntityBuilder(this).WithText(text);

    public ForwardEntityBuilder CreateForwardBuilder()
        => new(this);

    #endregion

    #region 一些api

    #region 获取消息
    IGroupMessage IClient.GetGroupMessageById(int messageId)
        => GetGroupMessageById(messageId);

    public GroupMessage GetGroupMessageById(int messageId)
        => GroupMessage.CreateFromMessageId(this, messageId);

    IPrivateMessage IClient.GetPrivateMessageById(int messageId)
        => GetPrivateMessageById(messageId);

    public PrivateMessage GetPrivateMessageById(int messageId)
        => PrivateMessage.CreateFromMessageId(this, messageId);

    IMessage IClient.GetMessageById(int messageId)
        => GetMessageById(messageId);

    public Message GetMessageById(int messageId)
        => Message.CreateFromMessageId(this, messageId);

    IFriendMessage IClient.GetFriendMessageById(int messageId)
        => GetFriendMessageById(messageId);

    public FriendMessage GetFriendMessageById(int messageId)
        => FriendMessage.CreateFromMessageId(this, messageId);
    #endregion

    #region 发消息

    #region 私聊

    async Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, long? groupId, IMessageEntity messageEntity)
        => await SendPrivateMessageAsync(userId, groupId, new MessageEntity(this, messageEntity)).ConfigureAwait(false);

    async Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, long? groupId, string rawString)
        => await SendPrivateMessageAsync(userId, groupId, rawString).ConfigureAwait(false);

    async Task<IFriendMessage> IClient.SendFriendMessageAsync(long friendUserId, IMessageEntity messageEntity)
        => await SendFriendMessageAsync(friendUserId, new MessageEntity(this, messageEntity)).ConfigureAwait(false);

    async Task<IFriendMessage> IClient.SendFriendMessageAsync(long friendUserId, string rawString)
        => await SendFriendMessageAsync(friendUserId, rawString).ConfigureAwait(false);


    /// <inheritdoc cref="IClient.SendPrivateMessageAsync(long, long?, IMessageEntity)"/>
    public async Task<PrivateMessage> SendPrivateMessageAsync(long userId, long? groupId, MessageEntity messageEntity)
    {
        SendPrivateMessageEntityAction api = new()
        {
            Message = messageEntity.Chain.ToModel(),
            UserId = userId,
            GroupId = groupId
        };
        var rst = await this.CallApiWithCheckingAsync(api).ConfigureAwait(false);

        PrivateMessage msg =
            PrivateMessage.CreateFromMessageId(this, rst.Data!.Cast<SendMessageActionResultData>().MessageId);
        return msg;
    }

    /// <inheritdoc cref="IClient.SendPrivateMessageAsync(long,long?, string)"/>
    public async Task<PrivateMessage> SendPrivateMessageAsync(long userId, long? groupId, string rawString)
    {
        SendPrivateMessageAction api = new()
        {
            Message = rawString,
            UserId = userId,
            GroupId = groupId
        };
        var rst = await this.CallApiWithCheckingAsync(api).ConfigureAwait(false);

        PrivateMessage msg =
            PrivateMessage.CreateFromMessageId(this, rst.Data!.Cast<SendMessageActionResultData>().MessageId);
        return msg;
    }

    /// <inheritdoc cref="IClient.SendFriendMessageAsync(long, IMessageEntity)"/>
    public async Task<FriendMessage> SendFriendMessageAsync(long friendUserId, MessageEntity messageEntity)
    {
        SendPrivateMessageEntityAction api = new()
        {
            Message = messageEntity.Chain.ToModel(),
            UserId = friendUserId
        };
        var rst = await this.CallApiWithCheckingAsync(api).ConfigureAwait(false);

        FriendMessage msg = FriendMessage.CreateFromMessageId(this, rst.Data!.Cast<SendMessageActionResultData>().MessageId);
        return msg;
    }

    /// <inheritdoc cref="IClient.SendFriendMessageAsync(long, IMessageEntity)"/>
    public async Task<FriendMessage> SendFriendMessageAsync(long friendUserId, string rawString)
    {
        SendPrivateMessageAction api = new()
        {
            Message = rawString,
            UserId = friendUserId
        };
        var rst = await this.CallApiWithCheckingAsync(api).ConfigureAwait(false);

        FriendMessage msg = FriendMessage.CreateFromMessageId(this, rst.Data!.Cast<SendMessageActionResultData>().MessageId);
        return msg;
    }

    #endregion

    #region 群聊
    async Task<IGroupMessage> IClient.SendGroupMessageAsync(long groupId, IMessageEntity messageEntity)
        => await SendGroupMessageAsync(groupId, new MessageEntity(this, messageEntity)).ConfigureAwait(false);

    async Task<IGroupMessage> IClient.SendGroupMessageAsync(long groupId, string rawString)
        => await SendGroupMessageAsync(groupId, rawString).ConfigureAwait(false);

    /// 使用消息实体发送群消息
    /// <inheritdoc cref="IClient.SendGroupMessageAsync(long, IMessageEntity)"/>
    public async Task<GroupMessage> SendGroupMessageAsync(long groupId, MessageEntity messageEntity)
    {
        SendGroupMessageEntityAction a = new()
        {
            GroupId = groupId,
            Message = messageEntity.Chain.ToModel()
        };
        var result = (await this.CallApiWithCheckingAsync(a).ConfigureAwait(false)).Data!.Cast<SendMessageActionResultData>();
        return GroupMessage.CreateFromMessageId(this, result.MessageId);
    }

    /// 使用原始消息字符串发送群消息
    /// <inheritdoc cref="IClient.SendGroupMessageAsync(long, string)"/>
    public async Task<GroupMessage> SendGroupMessageAsync(long groupId, string message)
    {
        SendGroupMessageAction a = new()
        {
            AsCqCodeString = true,
            GroupId = groupId,
            Message = message
        };
        var result = (await this.CallApiWithCheckingAsync(a).ConfigureAwait(false)).Data!.Cast<SendMessageActionResultData>();
        return GroupMessage.CreateFromMessageId(this, result.MessageId);
    }

    async Task<IGroupMessage> IClient.SendGroupMessageAsync(long groupId, IForwardEntity forwardEntity)
    {
        if (forwardEntity is ForwardEntity entity && ReferenceEquals(forwardEntity.Client, this))
            return await SendGroupMessageAsync(groupId, entity).ConfigureAwait(false);
        else
            throw new InvalidOperationException(StringConsts.NotSameClientError);
    }

    public async Task<GroupMessage> SendGroupMessageAsync(long groupId, ForwardEntity forwardEntity)
    {
        SendForwardMessageToGroupAction api = new()
        {
            GroupId = groupId,
            ForwardEntity = forwardEntity.ToModel()
        };
        var result = (await this.CallApiWithCheckingAsync<SendMessageActionResultData>(api).ConfigureAwait(false)).Item2;
        return this.GetGroupMessageById(result.MessageId);
    }

    Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, IForwardEntity forwardEntity)
        => throw new NotImplementedException("send forward as private msg is not impl");

    public async Task<FriendMessage> SendFriendMessageAsync(long friendUserId, ForwardEntity forwardEntity)
    {
        SendForwardMessageToUserAction api = new()
        {
            UserId = friendUserId,
            ForwardEntity = forwardEntity.ToModel()
        };
        var result = (await this.CallApiWithCheckingAsync<SendMessageActionResultData>(api).ConfigureAwait(false)).Item2;
        return this.GetFriendMessageById(result.MessageId);
    }

    async Task<IFriendMessage> IClient.SendFriendMessageAsync(long friendUserId, IForwardEntity forwardEntity)
        => await SendFriendMessageAsync(
            friendUserId,
            forwardEntity is ForwardEntity our ? our : throw new InvalidOperationException(StringConsts.NotSameClientError)
            ).ConfigureAwait(false);

    #endregion

    #region 通用

    Task IClient.RecallMessageAsync(int messageId)
        => RecallMessageAsync(messageId);

    public async Task RecallMessageAsync(int messageId)
    {
        DeleteMessageAction api = new()
        {
            MessageId = messageId
        };
        await this.CallApiWithCheckingAsync(api).ConfigureAwait(false);
    }

    #endregion

    #endregion

    #region 群的一些互动

    public Task BanGroupUserAsync(long groupId, long userId, TimeSpan time)
        => this.CallApiWithCheckingAsync(
            new BanGroupUserAction()
            {
                GroupId = groupId,
                UserId = userId,
                Duration = (int)time.TotalSeconds
            });

    public Task LiftBanGroupUserAsync(long groupId, long userId)
        => this.CallApiWithCheckingAsync(
            new BanGroupUserAction()
            {
                GroupId = groupId,
                UserId = userId,
                Duration = 0
            });

    public Task SetGroupNameAsync(long groupId, string newGroupName)
        => this.CallApiWithCheckingAsync(
            new SetGroupNameAction()
            {
                GroupId = groupId,
                GroupName = newGroupName
            });

    public Task SetGroupCardAsync(long groupId, long userId, string newCard)
        => this.CallApiWithCheckingAsync(
            new SetGroupCardAction()
            {
                GroupId = groupId,
                UserId = userId,
                Card = newCard
            }
            );

    #endregion

    #region 获取实体

    /// <summary>
    /// 获取一个群用户实体
    /// </summary>
    /// <param name="group">群</param>
    /// <param name="userId">用户Id</param>
    /// <returns>群用户实体</returns>
    public GroupUser GetGroupUser(JoinedGroup group, long userId)
        => GetGroupUser(group.GroupId, userId);

    /// <summary>
    /// 获取一个群用户实体
    /// </summary>
    /// <param name="groupId">群Id</param>
    /// <param name="user">用户</param>
    /// <returns>群用户实体</returns>
    public GroupUser GetGroupUser(long groupId, User user)
        => GetGroupUser(groupId, user.UserId);

    /// <summary>
    /// 获取一个群用户实体
    /// </summary>
    /// <param name="group">群</param>
    /// <param name="user">用户</param>
    /// <returns>群用户实体</returns>
    public GroupUser GetGroupUser(JoinedGroup group, User user)
        => GetGroupUser(group.GroupId, user.UserId);

    /// <summary>
    /// 获取一个群成员实体
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="userId">用户Id</param>
    public GroupUser GetGroupUser(long groupId, long userId)
        => GroupUser.CreateFromGroupIdAndUserId(this, groupId, userId);

    /// <summary>
    /// 获取一个用户实体
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns></returns>
    public User GetUser(long userId)
        => User.CreateFromId(this, userId);

    /// <summary>
    /// 获取一个群实体, 注意不会将返回值升级为JoinedGroup
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public Group GetGroup(long groupId)
        => Group.CreateFromGroupId(this, groupId);

    /// <summary>
    /// 获取一个bot加入的群的实体, 注意不会检查是否bot在群里
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public JoinedGroup GetJoinedGroup(long groupId)
        => JoinedGroup.CreateFromGroupId(this, groupId);

    public FriendUser GetFriendUser(long friendUserId)
        => FriendUser.CreateFromId(this, friendUserId);

    IGroup IClient.GetGroup(long groupId)
        => GetGroup(groupId);

    IJoinedGroup IClient.GetJoinedGroup(long groupId)
        => GetJoinedGroup(groupId);

    IUser IClient.GetUser(long userId)
        => GetUser(userId);

    IGroupUser IClient.GetGroupUser(long groupId, long userId)
        => GetGroupUser(groupId, userId);

    IFriendUser IClient.GetFriendUser(long friendUserId)
        => GetFriendUser(friendUserId);

    #endregion

    IUser IClient.Self => Self;

    IMessageEntityBuilder IClient.CreateMessageBuilder()
        => new MessageEntityBuilder(this);

    IForwardEntityBuilder IClient.CreateForwardBuilder()
        => new ForwardEntityBuilder(this);

    #region 亿堆事件


    event IClient.OnMessageReceivedHandler<IMessage>? IClient.OnMessageReceived
    {
        add => OnMessageReceived += value;
        remove => OnMessageReceived -= value;
    }

    event IClient.OnGroupMessageReceivedHandler<IGroupMessage, IJoinedGroup>? IClient.OnGroupMessageReceived
    {
        add => OnGroupMessageReceived += value;
        remove => OnGroupMessageReceived -= value;
    }

    event IClient.OnPrivateMessageReceivedHandler<IPrivateMessage, IUser>? IClient.OnPrivateMessageReceived
    {
        add => OnPrivateMessageReceived += value;
        remove => OnPrivateMessageReceived -= value;
    }

    event IClient.OnFriendMessageReceivedHandler<IFriendMessage, IFriendUser>? IClient.OnFriendMessageReceived
    {
        add => OnFriendMessageReceived += value;
        remove => OnFriendMessageReceived -= value;
    }

    event IClient.OnMessageRecalledHandler<IMessage, IUser>? IClient.OnMessageRecalled
    {
        add => OnMessageRecalled += value;
        remove => OnMessageRecalled -= value;
    }

    event IClient.OnPrivateMessageRecalledHandler<IPrivateMessage, IUser>? IClient.OnPrivateMessageRecalled
    {
        add => OnPrivateMessageRecalled += value;
        remove => OnPrivateMessageRecalled -= value;
    }

    event IClient.OnGroupMessageRecalledHandler<IGroupMessage, IJoinedGroup, IGroupUser>? IClient.OnGroupMessageRecalled
    {
        add => OnGroupMessageRecalled += value;
        remove => OnGroupMessageRecalled -= value;
    }

    event IClient.OnFriendAddedHandler<IFriendUser>? IClient.OnFriendAdded
    {
        add => OnFriendAdded += value;
        remove => OnFriendAdded -= value;
    }

    event IClient.OnGroupAdminChangedHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupAdminChanged
    {
        add => OnGroupAdminChanged += value;
        remove => OnGroupAdminChanged -= value;
    }

    event IClient.OnGroupAdminSetHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupAdminSet
    {
        add => OnGroupAdminSet += value;
        remove => OnGroupAdminSet -= value;
    }

    event IClient.OnGroupAdminCancelledHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupAdminCancelled
    {
        add => OnGroupAdminCancelled += value;
        remove => OnGroupAdminCancelled -= value;
    }

    event IClient.OnGroupEssenceSetHandler<IJoinedGroup, IGroupUser, IGroupMessage>? IClient.OnGroupEssenceSet
    {
        add => OnGroupEssenceSet += value;
        remove => OnGroupEssenceSet -= value;
    }

    event IClient.OnGroupEssenceAddedHandler<IJoinedGroup, IGroupUser, IGroupMessage>? IClient.OnGroupEssenceAdded
    {
        add => OnGroupEssenceAdded += value;
        remove => OnGroupEssenceAdded -= value;
    }

    event IClient.OnGroupEssenceRemovedHandler<IJoinedGroup, IGroupUser, IGroupMessage>? IClient.OnGroupEssenceRemoved
    {
        add => OnGroupEssenceRemoved += value;
        remove => OnGroupEssenceRemoved -= value;
    }

    event IClient.OnGroupFileUploadedHandler<IJoinedGroup, IGroupUser, IGroupFile>? IClient.OnGroupFileUploaded
    {
        add => OnGroupFileUploaded += value;
        remove => OnGroupFileUploaded -= value;
    }

    event IClient.OnOfflineFileReceivedHandler<IUser, IOfflineFile>? IClient.OnOfflineFileReceived
    {
        add => OnOfflineFileReceived += value;
        remove => OnOfflineFileReceived -= value;
    }

    event IClient.OnGroupMemberBannedHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupMemberBanned
    {
        add => OnGroupMemberBanned += value;
        remove => OnGroupMemberBanned -= value;
    }

    event IClient.OnGroupMemberBanLiftedHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupMemberBanLifted
    {
        add => OnGroupMemberBanLifted += value;
        remove => OnGroupMemberBanLifted -= value;
    }

    event IClient.OnGroupAllUserBannedHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupAllUserBanned
    {
        add => OnGroupAllUserBanned += value;
        remove => OnGroupAllUserBanned -= value;
    }

    event IClient.OnGroupAllUserBanLiftedHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupAllUserBanLifted
    {
        add => OnGroupAllUserBanLifted += value;
        remove => OnGroupAllUserBanLifted -= value;
    }

    event IClient.OnGroupMemberCardChangedHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupMemberCardChanged
    {
        add => OnGroupMemberCardChanged += value;
        remove => OnGroupMemberCardChanged -= value;
    }

    event IClient.OnGroupMemberChangedHandler<IJoinedGroup, IUser>? IClient.OnGroupMemberChanged
    {
        add => OnGroupMemberChanged += value;
        remove => OnGroupMemberChanged -= value;
    }

    event IClient.OnGroupMemberIncreasedHandler<IJoinedGroup, IGroupUser>? IClient.OnGroupMemberIncreased
    {
        add => OnGroupMemberIncreased += value;
        remove => OnGroupMemberIncreased -= value;
    }

    event IClient.OnGroupMemberDecreasedHandler<IJoinedGroup, IUser>? IClient.OnGroupMemberDecreased
    {
        add => OnGroupMemberDecreased += value;
        remove => OnGroupMemberDecreased -= value;
    }

    event IClient.OnFriendAddRequestedHandler<IFriendAddRequest>? IClient.OnFriendAddRequested
    {
        add => OnFriendAddRequested += value;
        remove => OnFriendAddRequested -= value;
    }

    event IClient.OnGroupJoinRequestedHandler<IGroupJoinRequest>? IClient.OnGroupJoinRequested
    {
        add => OnGroupJoinRequested += value;
        remove => OnGroupJoinRequested -= value;
    }

    event IClient.OnGroupInviteRequestedHandler<IGroupInviteRequest>? IClient.OnGroupInviteRequested
    {
        add => OnGroupInviteRequested += value;
        remove => OnGroupInviteRequested -= value;
    }

    #endregion

    #endregion
}
