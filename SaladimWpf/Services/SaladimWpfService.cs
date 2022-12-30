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
    protected MessagePipeline messagePipeline;

    public IClient Client { get; }

    public bool OpenGuessNumberBot { get; set; }

    public int GuessNumberBotDelay { get; set; }

    public SaladimWpfService(
        SaladimWpfServiceConfig config,
        SalLoggerService loggerService,
        SimCommandService simCommandService
        )
    {
        wsClient = new(config.GoCqHttpWebSocketAddress, LogLevel.Trace);
        Client = wsClient;
        logger = loggerService.SalIns;
        wsClient.OnLog += s => logger.LogInfo("WsClient", s);
        this.simCommandService = simCommandService;
        messagePipeline = new();
        ConfigurePipeline(messagePipeline);
        Client.OnMessageReceived += this.Client_OnMessageReceived;
    }

    private void ConfigurePipeline(MessagePipeline messagePipeline)
    {
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
#if DEBUG
                if (groupMsg.Group.GroupId != 860355679)
                    return;
#endif

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
    }

    private async void Client_OnMessageReceived(IMessage message)
    {
        await messagePipeline.ExecuteAsync(message).ConfigureAwait(false);
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
