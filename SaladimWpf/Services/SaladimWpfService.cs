using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.Core.Services;
using SaladimQBot.Extensions;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;

namespace SaladimWpf.Services;

public class SaladimWpfService : IClientService
{
    protected Logger logger;
    protected CqWebSocketClient wsClient;
    protected SimCommandService simCommandService;
    protected IServiceProvider serviceProvider;
    protected CoroutineService coroutineService;
    protected Pipeline<IIClientEvent> eventPipeline;
    protected Pipeline<IMessage> messagePipeline;

    public IClient Client { get; }

    public bool OpenGuessNumberBot { get; set; }

    public int GuessNumberBotDelay { get; set; }

    public SaladimWpfService(
        SaladimWpfServiceConfig config,
        SalLoggerService loggerService,
        SimCommandService simCommandService,
        IServiceProvider serviceProvider,
        CoroutineService coroutineService
        )
    {
        wsClient = new(config.GoCqHttpWebSocketAddress, LogLevel.Trace);
        Client = wsClient;
        logger = loggerService.SalIns;
        wsClient.OnLog += s => logger.LogInfo("WsClient", s);
        this.simCommandService = simCommandService;
        this.serviceProvider = serviceProvider;
        this.coroutineService = coroutineService;

        ConfigurePipeline(eventPipeline = new());
        ConfigureMessagePipeline(messagePipeline = new());
        Client.OnClientEventOccured += this.Client_OnClientEventOccured;
    }

    private void Client_OnClientEventOccured(IIClientEvent clientEvent)
    {
        Task.Run(() => eventPipeline.ExecuteAsync(clientEvent));
    }

    private void ConfigurePipeline(Pipeline<IIClientEvent> eventPipeline)
    {
        //转发消息处理给消息处理管线
        eventPipeline.AppendMiddleware(async (e, next) =>
        {
            if (e is ClientMessageReceivedEvent mre)
            {
                await messagePipeline.ExecuteAsync(mre.Message).ContinueWith(t =>
                {
                    if (t.Exception is not null)
                    {
                        logger.LogError("WpfClient", "MessagePipeline", t.Exception);
                    }
                }).ConfigureAwait(false);
            }
            await next().ConfigureAwait(false);
        });
        //协程处理中间件
        eventPipeline.AppendMiddleware(async (e, next) =>
        {
            coroutineService.PushCoroutines(e);
            await next().ConfigureAwait(false);
        });
    }

    private void ConfigureMessagePipeline(Pipeline<IMessage> messagePipeline)
    {
#if DEBUG
        //DEBUG时只接受测试群中间件
        messagePipeline.AppendMiddleware(async (msg, next) =>
        {
            if (msg is IGroupMessage groupMsg)
            {
                if (groupMsg.Group.GroupId == 860355679)
                    await next();
            }
        });
#endif
        /*
        //整活中间件
        messagePipeline.AppendMiddleware(async (msg, next) =>
        {
            if (msg is IGroupMessage groupMessage && groupMessage.MessageEntity.RawString == "签到")
            {
                await groupMessage.MessageWindow.SendMessageAsync(
                    msg.Client.CreateTextOnlyEntity("签到...失败, 根本没有签到功能哒!")
                    ).ConfigureAwait(false);
            }
            await next().ConfigureAwait(false);
        });*/
        //日志中间件
        messagePipeline.AppendMiddleware(async (msg, next) =>
        {
            if (msg is GroupMessage groupMsg)
            {
                logger.LogInfo(
                    "WpfConsole", $"{groupMsg.Group.Name.Value}({groupMsg.Group.GroupId}) - " +
                    $"{groupMsg.Author.FullName} 说: " +
                    $"{groupMsg.MessageEntity.RawString}"
                    .Replace(@"\", @"\\").Replace("\n", @"\n").Replace("\r", @"\r")
                    );
            }
            else if (msg is PrivateMessage privateMsg)
            {
                logger.LogInfo(
                    "WpfConsole", $"{await privateMsg.Sender.Nickname.GetValueAsync().ConfigureAwait(false)}" +
                    $"({privateMsg.Sender.UserId}) 私聊你: {privateMsg.MessageEntity.RawString}"
                    .Replace(@"\", @"\\").Replace("\n", @"\n").Replace("\r", @"\r")
                    );
            }
            await next();
        });

        //屏蔽临时消息中间件
        messagePipeline.AppendMiddleware(async (msg, next) =>
        {
            if (msg is not (GroupMessage or FriendMessage))
                return;
            await next();
        });

        //simCommand中间件
        messagePipeline.AppendMiddleware(async (msg, next) =>
        {
            await simCommandService.Executor.MatchAndExecuteAllAsync(msg).ConfigureAwait(false);
            await next();
        });

        //全自动1A2B消息中间件
        messagePipeline.AppendMiddleware(serviceProvider.GetRequiredService<Auto1A2BService>().Middleware);
    }

    public async Task StartAsync()
    {
        await wsClient.StartAsync().ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        await wsClient.StopAsync().ConfigureAwait(false);
    }
}
