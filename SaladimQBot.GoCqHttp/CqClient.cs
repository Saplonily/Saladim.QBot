using System.Diagnostics;
using System.Text.Json;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("CqClient, Started={Started}, StartedBefore={StartedBefore}")]
public abstract class CqClient : IClient, IExpirableValueGetter
{
    public abstract ICqSession ApiSession { get; }

    public abstract ICqSession PostSession { get; }

    public abstract TimeSpan ExpireTimeSpan { get; }

    /// <summary>
    /// 该Client之前是否尝试开启过
    /// </summary>
    public bool StartedBefore { get; protected set; }

    /// <summary>
    /// 该Client是否开启
    /// </summary>
    public bool Started { get; protected set; }

    protected Logger logger;
    protected readonly Dictionary<CqApi, Expirable<CqApiCallResultData>> cachedApiCallResultData = new();

    public CqClient(LogLevel logLevelLimit)
    {
        OnPost += InternalPostProcessor;
        logger = new LoggerBuilder()
                .WithLevelLimit(logLevelLimit)
               .WithAction(s => OnLog?.Invoke(s))
               .WithFormatter(ClientLogFormatter)
               .Build();

        static string ClientLogFormatter(LogLevel l, string s, string? ss, string content)
            => $"[" +
            $"{l}][{s}/{(ss is null ? "" : $"{ss}")}" +
            $"] {content}";
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
        return await ApiSession.CallApiAsync(api);
    }

    /// <summary>
    /// 使用该客户端调用原始Api, 同时转换返回的Data类型
    /// </summary>
    /// <typeparam name="T">api调用结果的Data(<see cref="CqApiCallResult.Data"/>)的类型</typeparam>
    /// <param name="api">要调用的api实体</param>
    /// <returns></returns>
    public async Task<(CqApiCallResult?, T?)> CallApiAsync<T>(CqApi api) where T : CqApiCallResultData
    {
        var result = await CallApiAsync(api);
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
            logger.LogInfo("Client", "Connection", "Connecting api session...");
            await ApiSession.StartAsync();
            logger.LogInfo("Client", "Connection", "Connecting post session...");
            await PostSession.StartAsync();

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
            logger.LogWarn("Client", "Connection", $"Connected failed. Please check the thrown Exception.");
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

    #region IExternalValueGetter

    Expirable<TChild> IExpirableValueGetter.MakeDependencyExpirable<TChild, TFather>(
            Expirable<TFather> dependentFather, Func<TFather, TChild> valueGetter
            )
            => MakeDependencyExpirable(dependentFather, valueGetter);

    Expirable<TChild> IExpirableValueGetter.MakeDependencyExpirable<TChild, TFather>(
        Expirable<TFather> dependentFather, TChild presetValue, Func<TFather, TChild> valueGetter
        )
        => MakeDependencyExpirable(dependentFather, presetValue, valueGetter);

    Expirable<TResultData> IExpirableValueGetter.MakeExpirableApiCallResultData<TResultData>(CqApi api)
        => MakeExpirableApiCallResultData<TResultData>(api);

    Expirable<T> IExpirableValueGetter.MakeNoneExpirableExpirable<T>(Func<T> valueFactory)
        => MakeNoneExpirableExpirable(valueFactory);

    #endregion

    #region Expirable<T> 和 DependencyExpirable Maker

    internal Expirable<TResultData> MakeExpirableApiCallResultData<TResultData>(CqApi api) where TResultData : CqApiCallResultData
    {
        Expirable<CqApiCallResultData> ex;
        bool cached = cachedApiCallResultData.TryGetValue(api, out var exFound);
        if (cached)
        {
            ex = exFound!;
        }
        else
        {
            ex = new(ApiCallResultDataFactory, this.ExpireTimeSpan);
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
        return CastedExpirable<TResultData, CqApiCallResultData>.MakeFromSource(ex!);
        CqApiCallResultData ApiCallResultDataFactory()
            => this.CallApiImplicitlyWithCheckingAsync(api).Result.Data!;

    }

    internal Expirable<TChild> MakeDependencyExpirable<TChild, TFather>(
        Expirable<TFather> dependentFather,
        Func<TFather, TChild> valueGetter
        ) where TChild : notnull where TFather : notnull
    {
        Expirable<TChild> child;
        child = new(DependencyExpirableFactory, dependentFather.ExpireTime, dependentFather.TimeSpanExpired);
        return child;
        TChild DependencyExpirableFactory()
        {
            return valueGetter(dependentFather.Value);
        }
    }

    internal Expirable<TChild> MakeDependencyExpirable<TChild, TFather>(
        Expirable<TFather> dependentFather,
        TChild presetValue,
        Func<TFather, TChild> valueGetter
        ) where TChild : notnull where TFather : notnull
    {
        Expirable<TChild> child;
        child = new(DependencyExpirableFactory, presetValue, dependentFather.ExpireTime, dependentFather.TimeSpanExpired);
        return child;
        TChild DependencyExpirableFactory()
        {
            return valueGetter(dependentFather.Value);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<挂起>")]
    internal Expirable<T> MakeNoneExpirableExpirable<T>(Func<T> valueFactory) where T : notnull
        => new(valueFactory, TimeSpan.FromTicks(-1));

    #endregion

    internal void InternalPostProcessor(CqPost post)
    {
        switch (post)
        {
            case CqMessagePost messagePost:
                switch (messagePost)
                {
                    case CqGroupMessagePost groupMessagePost:
                        GroupMessage gm = GroupMessage.CreateFromGroupMessagePost(this, groupMessagePost);
                        OnMessageReceived?.Invoke(gm);
                        OnGroupMessageReceived?.Invoke(gm, gm.Group);
                        break;

                    case CqPrivateMessagePost privateMessagePost:
                        PrivateMessage pm = PrivateMessage.CreateFromPrivateMessagePost(this, privateMessagePost);
                        OnMessageReceived?.Invoke(pm);
                        OnPrivateMessageReceived?.Invoke(pm, pm.PrivateSender);
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
                            OnFriendAdded?.Invoke(this.GetUser(notice.UserId));
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
                            GroupUser user = this.GetGroupUser(notice.GroupId, notice.UserId);
                            GroupUser operatorUser = this.GetGroupUser(notice.GroupId, notice.OperatorId);
                            JoinedGroup group = this.GetJoinedGroup(notice.GroupId);

                            if (isBan)
                                OnGroupMemberBanned?.Invoke(group, user, operatorUser, TimeSpan.FromSeconds(notice.Duration));
                            else
                                OnGroupMemberLiftBan?.Invoke(group, user, operatorUser);
                        }
                        break;

                    case CqGroupMemberCardChangedNoticePost notice:
                        {
                            GroupUser user = this.GetGroupUser(notice.GroupId, notice.UserId);
                            JoinedGroup group = this.GetJoinedGroup(notice.GroupId);
                            //TODO 重构可过期类型(Expirable), 将依赖过期写入Expirable类中
                            //以能够在上游调用强制过期时下游知道自己过期了
                            //在这里的例子是群员群名片更改时得让get_group_member_info的所有下游都过期
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


        }
    }

    #region 一堆用户层的事件

    #region 消息收到
    public delegate void OnMessageReceivedHandler(Message message);
    public event OnMessageReceivedHandler? OnMessageReceived;
    public delegate void OnGroupMessageReceivedHandler(GroupMessage message, JoinedGroup group);
    public event OnGroupMessageReceivedHandler? OnGroupMessageReceived;
    public delegate void OnPrivateMessageReceivedHandler(PrivateMessage message, User user);
    public event OnPrivateMessageReceivedHandler? OnPrivateMessageReceived;
    #endregion

    #region 消息撤回
    public delegate void OnMessageRecalledHandler(Message message, User operatorUser);
    public event OnMessageRecalledHandler? OnMessageRecalled;
    public delegate void OnPrivateMessageRecalledHandler(PrivateMessage message, User user);
    public event OnPrivateMessageRecalledHandler? OnPrivateMessageRecalled;
    public delegate void OnGroupMessageRecalledHandler(GroupMessage message, JoinedGroup group, GroupUser operatorUser);
    public event OnGroupMessageRecalledHandler? OnGroupMessageRecalled;
    #endregion

    #region Notice收到
    //好友添加
    public delegate void OnFriendAddedHandler(User user);
    public event OnFriendAddedHandler? OnFriendAdded;

    //群管理员变动
    public delegate void OnGroupAdminChangedHandler(JoinedGroup group, GroupUser user, bool isSet);
    public event OnGroupAdminChangedHandler? OnGroupAdminChanged;
    public delegate void OnGroupAdminSetHandler(JoinedGroup group, GroupUser user);
    public event OnGroupAdminSetHandler? OnGroupAdminSet;
    public delegate void OnGroupAdminCancelledHandler(JoinedGroup group, GroupUser user);
    public event OnGroupAdminCancelledHandler? OnGroupAdminCancelled;

    //群精华变动
    public delegate void OnGroupEssenceSetHandler(JoinedGroup group, GroupUser operatorUser, GroupUser user, GroupMessage message, bool isAdd);
    public event OnGroupEssenceSetHandler? OnGroupEssenceSet;
    public delegate void OnGroupEssenceAddedHandler(JoinedGroup group, GroupUser operatorUser, GroupUser user, GroupMessage message);
    public event OnGroupEssenceAddedHandler? OnGroupEssenceAdded;
    public delegate void OnGroupEssenceRemovedHandler(JoinedGroup group, GroupUser operatorUser, GroupUser user, GroupMessage message);
    public event OnGroupEssenceRemovedHandler? OnGroupEssenceRemoved;

    //群文件上传
    public delegate void OnGroupFileUploadedHandler(JoinedGroup group, GroupUser uploader, GroupFile groupFile);
    public event OnGroupFileUploadedHandler? OnGroupFileUploaded;

    //群禁言
    public delegate void OnGroupMemberBannedHandler(JoinedGroup group, GroupUser groupUser, GroupUser operatorUser, TimeSpan timeSpan);
    public event OnGroupMemberBannedHandler? OnGroupMemberBanned;
    public delegate void OnGroupMemberLiftBanHandler(JoinedGroup group, GroupUser groupUser, GroupUser operatorUser);
    public event OnGroupMemberLiftBanHandler? OnGroupMemberLiftBan;

    //群名片变更
    public delegate void OnGroupMemberCardChangedHandler(JoinedGroup group, GroupUser groupUser, string from, string to);
    public event OnGroupMemberCardChangedHandler? OnGroupMemberCardChanged;

    //离线文件收到
    public delegate void OnOfflineFileReceivedHandler(User user, OfflineFile file);
    public event OnOfflineFileReceivedHandler? OnOfflineFileReceived;

    //群成员变更
    public delegate void OnGroupMemberChangedHandler(JoinedGroup group, User user, bool isIncrease);
    public event OnGroupMemberChangedHandler? OnGroupMemberChanged;
    public delegate void OnGroupMemberIncreasedHandler(JoinedGroup group, GroupUser user);
    public event OnGroupMemberIncreasedHandler? OnGroupMemberIncreased;
    public delegate void OnGroupMemberDecreasedHandler(JoinedGroup group, User user);
    public event OnGroupMemberDecreasedHandler? OnGroupMemberDecreased;
    #endregion

    #endregion

    #region IClient

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
    #endregion

    #region 发消息

    #region 私聊

    async Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, IMessageEntity messageEntity)
        => await SendPrivateMessageAsync(userId, new MessageEntity(this, messageEntity));

    async Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, string rawString)
        => await SendPrivateMessageAsync(userId, rawString);

    /// <inheritdoc cref="IClient.SendPrivateMessageAsync(long, IMessageEntity)"/>
    public async Task<PrivateMessage> SendPrivateMessageAsync(long userId, MessageEntity messageEntity)
    {
        SendPrivateMessageEntityAction api = new()
        {
            Message = messageEntity.Chain.ToModel(),
            UserId = userId
        };
        var rst = await this.CallApiWithCheckingAsync(api);

        PrivateMessage msg =
            PrivateMessage.CreateFromMessageId(this, rst.Data!.Cast<SendMessageActionResultData>().MessageId);
        return msg;
    }

    /// <inheritdoc cref="IClient.SendPrivateMessageAsync(long, string)"/>
    public async Task<PrivateMessage> SendPrivateMessageAsync(long userId, string rawString)
    {
        SendPrivateMessageAction api = new()
        {
            Message = rawString,
            UserId = userId
        };
        var rst = await this.CallApiWithCheckingAsync(api);

        PrivateMessage msg =
            PrivateMessage.CreateFromMessageId(this, rst.Data!.Cast<SendMessageActionResultData>().MessageId);
        return msg;
    }

    #endregion

    #region 群聊
    async Task<IGroupMessage> IClient.SendGroupMessageAsync(long groupId, IMessageEntity messageEntity)
        => await SendGroupMessageAsync(groupId, new MessageEntity(this, messageEntity));

    async Task<IGroupMessage> IClient.SendGroupMessageAsync(long groupId, string rawString)
        => await SendGroupMessageAsync(groupId, rawString);

    /// <inheritdoc cref="IClient.SendGroupMessageAsync(long, IMessageEntity)"/>
    public async Task<GroupMessage> SendGroupMessageAsync(long groupId, MessageEntity messageEntity)
    {
        SendGroupMessageEntityAction a = new()
        {
            GroupId = groupId,
            Message = messageEntity.Chain.ToModel()
        };
        var result = (await this.CallApiWithCheckingAsync(a)).Data!.Cast<SendMessageActionResultData>();
        return GroupMessage.CreateFromMessageId(this, result.MessageId);
    }

    /// <inheritdoc cref="IClient.SendGroupMessageAsync(long, string)"/>
    public async Task<GroupMessage> SendGroupMessageAsync(long groupId, string message)
    {
        SendGroupMessageAction a = new()
        {
            AsCqCodeString = true,
            GroupId = groupId,
            Message = message
        };
        var result = (await this.CallApiWithCheckingAsync(a)).Data!.Cast<SendMessageActionResultData>();
        return GroupMessage.CreateFromMessageId(this, result.MessageId);
    }

    async Task<IGroupMessage> IClient.SendGroupMessageAsync(long groupId, IForwardEntity forwardEntity)
    {
        if (forwardEntity is ForwardEntity entity && ReferenceEquals(forwardEntity.Client, this))
            return await SendGroupMessageAsync(groupId, entity);
        else
            throw new InvalidOperationException("Only accept send forwardEntity that this client own.");
    }

    public async Task<GroupMessage> SendGroupMessageAsync(long groupId, ForwardEntity forwardEntity)
    {
        SendForwardMessageToGroupAction api = new()
        {
            GroupId = groupId,
            ForwardEntity = forwardEntity.ToModel()
        };
        var result = (await this.CallApiWithCheckingAsync(api)).Data!.Cast<SendMessageActionResultData>();
        return this.GetGroupMessageById(result.MessageId);
    }

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
        await this.CallApiWithCheckingAsync(api);
    }

    #endregion

    #endregion

    #region 群的一些互动

    public async Task BanGroupUserAsync(long groupId, long userId, TimeSpan time)
    {
        BanGroupUserAction api = new()
        {
            GroupId = groupId,
            UserId = userId,
            Duration = (int)time.TotalSeconds
        };
        await this.CallApiWithCheckingAsync(api);
    }

    public async Task LiftBanGroupUserAsync(long groupId, long userId)
    {
        BanGroupUserAction api = new()
        {
            GroupId = groupId,
            UserId = userId,
            Duration = 0
        };
        await this.CallApiWithCheckingAsync(api);
    }

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

    IGroup IClient.GetGroup(long groupId)
        => GetGroup(groupId);

    IJoinedGroup IClient.GetJoinedGroup(long groupId)
        => GetJoinedGroup(groupId);

    IUser IClient.GetUser(long userId)
        => GetUser(userId);

    IGroupUser IClient.GetGroupUser(long groupId, long userId)
        => GetGroupUser(groupId, userId);

    #endregion

    #endregion
}
