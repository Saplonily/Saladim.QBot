using System.Net.WebSockets;
using System.Text.Json;
using Saladim.SalLogger;
using SaladimQBot.Shared;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using System.Text.RegularExpressions;

namespace SaladimQBot.GoCqHttp;

public sealed class CqWebSocketClient : ICqClient, IAsyncDisposable
{
    #region 私有字段 & 常量属性

    private readonly CqWebSocketSession sessionPost;
    private readonly CqWebSocketSession sessionApi;
    private readonly Dictionary<CqApi, Expirable<CqApiCallResultData>> cachedApiCallResultData = new();
    private readonly Logger logger;
    private int seq = int.MinValue;

    public TimeSpan ExpireTimeSpan { get; } = new TimeSpan(0, 3, 0);

    #endregion

    #region 构造器
    /// <summary>
    /// 创建一个Cq WebSocket客户端
    /// </summary>
    /// <param name="gocqhttpWsAddress">
    /// <para>go-cqhttp的websocket地址(包含端口号,不包含协议前缀)</para>
    /// <para>例如: "127.0.0.1:5000"</para>
    /// </param>
    /// <param name="logLevelLimit">客户端输出日志限制等级, 默认为Info等级</param>
    public CqWebSocketClient(string gocqhttpWsAddress, LogLevel logLevelLimit = LogLevel.Info)
    {
        sessionApi = new(gocqhttpWsAddress, StringConsts.CqApiEndpoint, false, true);
        sessionPost = new(gocqhttpWsAddress, StringConsts.CqPostEndpoint, true, false);
        logger = new LoggerBuilder()
            .WithLevelLimit(logLevelLimit)
            .WithAction(s => OnLog?.Invoke(s))
            .WithFormatter(ClientLogFormatter)
            .Build();
        OnPost += InternalPostProcesser;
        static string ClientLogFormatter(LogLevel l, string s, string? ss, string content)
            => $"[{l}][{s}/{(ss is null ? "" : $"{ss}")}] {content}";
    }
    #endregion

    #region Client状态

    /// <summary>
    /// 在<see cref="StartAsync"/>执行成功后变为true,之后不再变化
    /// </summary>
    public bool StartedBefore { get; private set; }

    /// <summary>
    /// 该Client是否开启
    /// </summary>
    public bool Started { get; private set; }

    #endregion

    #region 两个Session,OnPost和OnLog事件
    public ICqSession ApiSession => sessionApi;

    public ICqSession PostSession => sessionPost;

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
        if (logger.NeedLogging(LogLevel.Trace))
        {
            var str = JsonSerializer.Serialize(api, api.GetType(), CqJsonOptions.Instance);
            logger.LogTrace("Client", "ApiCall", $"ready for api call({api.ApiName}) : {str}");
        }
        return await sessionApi.CallApiAsync(api, $"{seq++}");
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
        return InternalStopAsync();
    }

    #endregion

    #region Internal Start & Stops 实现

    internal async Task InternalStartAsync()
    {
        void CheckAndRenewWs(CqWebSocketSession s)
        {
            if (s.State is WebSocketState.Closed or WebSocketState.Aborted)
                s.RenewWebSocket();
            logger.LogDebug("Client", "Connection", "Renewing WebSocketSession which has been closed or aborted.");
        }

        ObjectHelper.BulkRun(CheckAndRenewWs, sessionApi, sessionPost);
        try
        {
            CancellationToken token = CancellationToken.None;
            logger.LogInfo("Client", "Connection", "Connecting WebSocketSession...");
            await ObjectHelper.BulkRunAsync(async s => await s.ConnectAsync(token), sessionApi, sessionPost);

            _ = sessionPost.StartReceiving().ContinueWith(ExceptionChecker);
            sessionPost.OnReceived += (in JsonDocument srcDoc) =>
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
            };
            _ = sessionApi.StartReceiving().ContinueWith(ExceptionChecker);

            ObjectHelper.BulkRun(s => s.OnReceivedAcceptableException += OnReceivedAcceptableException, sessionApi, sessionPost);
            void OnReceivedAcceptableException(Exception e)
            {
                if (e is CqPostLoadFailedExcpetion postLoadFailedE)
                {
                    if (logger.NeedLogging(LogLevel.Warn))
                        logger.LogWarn("Client", "PostReceive", $"Failed to parse post string. Detailed message: " +
                            $"{postLoadFailedE.Message}, inner json deserializer message: " +
                            $"{postLoadFailedE.InnerException!.Message}");
                }
            }
            StartedBefore = true;
            Started = true;
        }
        catch (WebSocketException wex)
        {
            Started = false;
            var msg = $"Internal WebSocket error({wex.WebSocketErrorCode}), " +
                $"please check the inner exceptions.";
            logger.LogWarn("Client", "Connection", $"Connected failed. Detailed message: {msg}");
            throw new ClientException(this,
                ClientException.ExceptionType.WebSocketError,
                innerException: wex,
                message: msg
                );
        }
        return;

        void ExceptionChecker(Task task)
        {
            if (task.Exception is (not null) and AggregateException ae)
            {
                logger.LogWarn("Client", "ApiCall", ae);
            }
        }
    }

    internal async Task InternalStopAsync()
    {
        if (Started)
        {
            logger.LogInfo("Client", "Connection", "Stoping connection...");
            await ObjectHelper.BulkRunAsync(s => s.StopReceiving(), sessionApi, sessionPost);
            Started = false;
        }
        return;
    }


    #endregion

    #region IDisposeable

    /// <summary>
    /// 异步dispose客户端(同StopAsync())
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        await InternalStopAsync();
        return;
    }

    #endregion

    #region IExternalValueGetter

    Expirable<TChild> IExpirableValueGetter.MakeDependencyExpirable<TChild, TFather>(Expirable<TFather> dependentFather, Func<TFather, TChild> valueGetter)
        => MakeDependencyExpirable(dependentFather, valueGetter);

    Expirable<TChild> IExpirableValueGetter.MakeDependencyExpirable<TChild, TFather>(Expirable<TFather> dependentFather, TChild presetValue, Func<TFather, TChild> valueGetter)
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
            => this.CallApiImplicityWithCheckingAsync(api).Result.Data!;

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

    #region 本地的OnPost处理器 (提供IUser IGroup等实例)

    internal void InternalPostProcesser(CqPost post)
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

    #endregion

    #region 一堆用户层的事件

    public event Action<Message>? OnMessageReceived;
    public event Action<GroupMessage>? OnGroupMessageReceived;
    public event Action<PrivateMessage>? OnPrivateMessageReceived;

    #endregion

    #region IClient

    #region 获取消息
    IGroupMessage IClient.GetGroupMessageById(long messageId)
        => GetGroupMessageById(messageId);

    public GroupMessage GetGroupMessageById(long messageId)
        => GroupMessage.CreateFromMessageId(this, messageId);

    IPrivateMessage IClient.GetPrivateMessageById(long messageId)
        => GetPrivateMessageById(messageId);

    public PrivateMessage GetPrivateMessageById(long messageId)
        => PrivateMessage.CreateFromMessageId(this, messageId);
    #endregion

    #region 发消息
    async Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, IMessageEntity messageEntity)
        => await SendPrivateMessageAsync(userId, new MessageEntity(messageEntity));

    async Task<IPrivateMessage> IClient.SendPrivateMessageAsync(long userId, string rawString)
        => await SendPrivateMessageAsync(userId, rawString);

    /// <inheritdoc cref="IClient.SendPrivateMessageAsync(long, IMessageEntity)"/>
    public async Task<PrivateMessage> SendPrivateMessageAsync(long userId, MessageEntity messageEntity)
    {
        SendPrivateMessageEntityAction api = new()
        {
            Message = messageEntity.cqEntity,
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
            Message = messageEntity.cqEntity
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

    #endregion
}