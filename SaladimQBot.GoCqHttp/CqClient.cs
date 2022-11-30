using System.Text.Json;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public abstract class CqClient : ICqClient, IExpirableValueGetter
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
            => $"[{l}][{s}/{(ss is null ? "" : $"{ss}")}] {content}";
    }

    #region OnPost和OnLog事件

    /// <summary>
    /// <para>收到原始上报时发生,CqPost类型参数为实际实体上报类</para>
    /// <para>事件源以「同步」方式触发此事件</para>
    /// </summary>
    public event Action<CqPost> OnPost;

    /// <summary>
    /// <para>客户端日志事件</para>
    /// </summary>
    public event Action<string>? OnLog;

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
        CqPost? post = JsonSerializer.Deserialize<CqPost>(srcDoc, CqJsonOptions.Instance);
        if (post is null)
        {
            if (logger.NeedLogging(LogLevel.Warn))
                logger.LogWarn("Client", "PostReceive", $"Deserialize CqPost failed. " +
                    $"Raw post string is:\n{JsonSerializer.Serialize(srcDoc)}");
            return;
        };
        OnPost?.Invoke(post);
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

    internal Expirable<T> MakeNoneExpirableExpirable<T>(Func<T> valueFactory) where T : notnull
        => new(valueFactory, TimeSpan.FromTicks(-1));

    #endregion

    internal void InternalPostProcessor(CqPost post)
    {
        switch (post)
        {
            case CqMessagePost messagePost:
                switch (post)
                {
                    case CqGroupMessagePost groupMessagePost:
                        GroupMessage gm = GroupMessage.CreateFromGroupMessagePost(this, groupMessagePost);
                        OnMessageReceived?.Invoke(gm);
                        OnGroupMessageReceived?.Invoke(gm);
                        break;

                    case CqPrivateMessagePost privateMessagePost:
                        PrivateMessage pm = PrivateMessage.CreateFromPrivateMessagePost(this, privateMessagePost);
                        OnMessageReceived?.Invoke(pm);
                        OnPrivateMessageReceived?.Invoke(pm);
                        break;
                }
                break;
        }
    }

    #region 一堆用户层的事件

    public event Action<Message>? OnMessageReceived;
    public event Action<GroupMessage>? OnGroupMessageReceived;
    public event Action<PrivateMessage>? OnPrivateMessageReceived;

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
    #endregion

    #region 发消息

    #region 私聊

    async Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, IMessageEntity messageEntity)
        => await SendPrivateMessageAsync(userId, new MessageEntity(messageEntity));

    async Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, string rawString)
        => await SendPrivateMessageAsync(userId, rawString);

    /// <inheritdoc cref="IClient.SendPrivateMessageAsync(long, IMessageEntity)"/>
    public async Task<PrivateMessage> SendPrivateMessageAsync(long userId, MessageEntity messageEntity)
    {
        SendPrivateMessageEntityAction api = new()
        {
            Message = messageEntity.cqChainEntity,
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
        => await SendGroupMessageAsync(groupId, new MessageEntity(messageEntity));

    async Task<IGroupMessage> IClient.SendGroupMessageAsync(long groupId, string rawString)
        => await SendGroupMessageAsync(groupId, rawString);

    /// <inheritdoc cref="IClient.SendGroupMessageAsync(long, IMessageEntity)"/>
    public async Task<GroupMessage> SendGroupMessageAsync(long groupId, MessageEntity messageEntity)
    {
        SendGroupMessageEntityAction a = new()
        {
            GroupId = groupId,
            Message = messageEntity.cqChainEntity
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

    IGroupUser IClient.GetGroupUser(long groupId, long userId)
        => GetGroupUser(groupId, userId);

    #endregion

    #endregion
}
