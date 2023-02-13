using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.Extensions;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;


namespace Saladim.Offbot.Services;

public class SaladimOffbotService : IHostedService
{
    protected Logger logger;
    protected CqWebSocketClient wsClient;
    protected SimCommandService simCommandService;
    protected IServiceProvider serviceProvider;
    protected CoroutineService coroutineService;
    protected Pipeline<IClientEvent> eventPipeline;
    protected Pipeline<IMessage> messagePipeline;

    public IClient Client { get; }

    public SaladimOffbotService(
        SaladimOffbotServiceConfig config,
        IServiceProvider serviceProvider,
        SalLoggerService loggerService,
        SimCommandService simCommandService,
        CoroutineService coroutineService
        )
    {
        wsClient = new(config.GoCqHttpWebSocketAddress, LogLevel.Trace, authorization: "d7ffb205-632d-4e9c-8922-236ea7a3d08c");
        Client = wsClient;
        logger = loggerService.SalIns;
        wsClient.OnLog += s => logger.LogInfo("WsClient", s);
        this.simCommandService = simCommandService;
        this.serviceProvider = serviceProvider;
        this.coroutineService = coroutineService;
        ConfigurePipeline(eventPipeline = new());
        ConfigureMessagePipeline(messagePipeline = new());
        Client.OnClientEventOccurred += this.Client_OnClientEventOccurred;
        Client.OnStoppedUnexpectedly += Client_OnStoppedUnexpectedly;
    }

    private void Client_OnStoppedUnexpectedly(Exception ce)
    {
        Task.Run(async () =>
        {
            logger.LogError("Offbot", ce, $"Client session stopped unexpectetedly! " +
                $"Next connection try will be started in 2s.");
            Thread.Sleep(2000);
            try
            {
                await Client.StartAsync();
            }
            catch (ClientException e)
            {
                logger.LogError("Offbot", e, "Error when trying to reconnect the saladimOffbot service");
            }
        });
    }

    private void Client_OnClientEventOccurred(IClientEvent clientEvent)
    {
        Task.Run(() => eventPipeline.ExecuteAsync(clientEvent));
    }

    private void ConfigurePipeline(Pipeline<IClientEvent> eventPipeline)
    {
        //异常中间件
        eventPipeline.AppendMiddleware(async (e, next) =>
        {
            try
            {
                await next().ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                logger.LogError("Offbot", exc, "Error detected by exception middleware:");
            }
        });
        //转发消息处理给消息处理管线
        eventPipeline.AppendMiddleware(async (e, next) =>
        {
            do
            {
                if (e is IClientMessageReceivedEvent mre)
                {
#if DEBUG
                    if (mre.Message is IGroupMessage groupMsg && groupMsg.Group.GroupId != 860355679)
                        break;
#endif
                    await messagePipeline.ExecuteAsync(mre.Message).ContinueWith(t =>
                    {
                        if (t.Exception is not null)
                        {
                            logger.LogError("Offbot", "MessagePipeline", t.Exception);
                        }
                    }).ConfigureAwait(false);
                }
                await next().ConfigureAwait(false);
            }
            while (false);
        });
        //协程处理中间件
        eventPipeline.AppendMiddleware(async (e, next) =>
        {
            coroutineService.PushCoroutines(e);
            await next().ConfigureAwait(false);
        });
        //同意好友、群消息中间件
        eventPipeline.AppendMiddleware(async (e, next) =>
        {
            if (e is IClientFriendAddRequestedEvent eFriend)
            {
                string comment = eFriend.Request.Comment;
                byte[] md5Result = MD5.HashData(Encoding.UTF8.GetBytes(eFriend.Request.User.UserId + "offKey"));
                string expectKey = string.Concat(md5Result.Select(b => b.ToString("0x")))[..4];
                bool approve = false;
                if (expectKey.Equals(comment, StringComparison.InvariantCultureIgnoreCase))
                {
                    await eFriend.Request.ApproveAsync().ConfigureAwait(false);
                    approve = true;
                }
                else
                {
                    await eFriend.Request.DisapproveAsync().ConfigureAwait(false);
                    approve = false;
                }
                string log = $"{eFriend.Request.User.Nickname} 请求添加好友, " +
                    $"期望key值: {expectKey}, 实际给出: {comment}, 是否同意: {approve}";
                logger.LogInfo("Offbot", log);
            }
        });
    }

    private void ConfigureMessagePipeline(Pipeline<IMessage> messagePipeline)
    {
        //日志中间件
        messagePipeline.AppendMiddleware(LogMiddleware);

        //屏蔽临时消息中间件
        messagePipeline.AppendMiddleware(IgnoreTempMsgMiddleware);

        //simCommand中间件
        messagePipeline.AppendMiddleware(SimCmdMiddleware);

        //全自动1A2B消息中间件
        messagePipeline.AppendMiddleware(serviceProvider.GetRequiredService<Auto1A2BService>().Middleware);
    }

    private async Task LogMiddleware(IMessage msg, Func<Task> next)
    {
        if (msg is GroupMessage groupMsg)
        {
            logger.LogInfo(
                "Offbot", $"{groupMsg.Group.Name.Value}({groupMsg.Group.GroupId}) - " +
                $"{groupMsg.Author.FullName} 说: " +
                $"{groupMsg.MessageEntity.RawString}"
                .Replace(@"\", @"\\").Replace("\n", @"\n").Replace("\r", @"\r")
                );
        }
        else if (msg is PrivateMessage privateMsg)
        {
            logger.LogInfo(
                "Offbot", $"{await privateMsg.Sender.Nickname.GetValueAsync().ConfigureAwait(false)}" +
                $"({privateMsg.Sender.UserId}) 私聊你: {privateMsg.MessageEntity.RawString}"
                .Replace(@"\", @"\\").Replace("\n", @"\n").Replace("\r", @"\r")
                );
        }
        await next();
    }

    private async Task SimCmdMiddleware(IMessage msg, Func<Task> next)
    {
        await simCommandService.Executer.MatchAndExecuteAllAsync(msg).ConfigureAwait(false);
        await next();
    }

    private async Task IgnoreTempMsgMiddleware(IMessage msg, Func<Task> next)
    {
        if (msg is not (GroupMessage or FriendMessage)) return;
        await next();
    }

    public async Task StartAsync(CancellationToken token)
    {
        await wsClient.StartAsync().ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken token)
    {
        await wsClient.StopAsync().ConfigureAwait(false);
    }
}
