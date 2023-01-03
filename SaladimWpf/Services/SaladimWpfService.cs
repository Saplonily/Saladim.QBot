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
    protected Pipeline<IMessage> messagePipeline;

    public IClient Client { get; }

    public bool OpenGuessNumberBot { get; set; }

    public int GuessNumberBotDelay { get; set; }

    public SaladimWpfService(
        SaladimWpfServiceConfig config,
        SalLoggerService loggerService,
        SimCommandService simCommandService,
        IServiceProvider serviceProvider
        )
    {
        wsClient = new(config.GoCqHttpWebSocketAddress, LogLevel.Trace);
        Client = wsClient;
        logger = loggerService.SalIns;
        wsClient.OnLog += s => logger.LogInfo("WsClient", s);
        this.simCommandService = simCommandService;
        this.serviceProvider = serviceProvider;
        messagePipeline = new();
        ConfigurePipeline(messagePipeline);
        Client.OnClientEventOccured += this.Client_OnClientEventOccured;
    }

    private void Client_OnClientEventOccured(IIClientEvent clientEvent)
    {
        if (clientEvent is ClientMessageReceivedEvent e)
        {
            Client_OnMessageReceived(e.Message);
        }
    }

    private void ConfigurePipeline(Pipeline<IMessage> messagePipeline)
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

    private void Client_OnMessageReceived(IMessage message)
    {
        messagePipeline.ExecuteAsync(message);
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
