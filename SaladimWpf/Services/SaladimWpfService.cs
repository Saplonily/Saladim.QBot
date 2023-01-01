using System.Diagnostics;
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

    protected IJoinedGroup? auto1A2BDoingGroup = null;
    protected HashSet<string> currentAuto1A2BHashSet = new();
    protected char[]? auto1A2BLastGuess = null;

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
        //全自动1A2B消息中间件
        messagePipeline.AppendMiddleware(Automatic1A2BMiddleware);
    }

    private async Task Automatic1A2BMiddleware(IMessage msg, Func<Task> next)
    {
        if (msg is IGroupMessage groupMessage)
        {
            var me = msg.MessageEntity;
            var msgStr = me.RawString;
            if (msgStr.StartsWith("开始自动1a2b", StringComparison.OrdinalIgnoreCase))
            {
                if (auto1A2BDoingGroup is null)
                {
                    currentAuto1A2BHashSet = new();
                    auto1A2BLastGuess = "1234".ToArray();
                    auto1A2BDoingGroup = groupMessage.Group;
                    await msg.MessageWindow.SendMessageAsync("开始1a2b").ConfigureAwait(false);
                    await msg.MessageWindow.SendMessageAsync("我猜1234").ConfigureAwait(false);
                }
                else
                {
                    await msg.MessageWindow.SendMessageAsync("自动1a2b机已经在其他群开启了哦~").ConfigureAwait(false);
                }
            }
            if (msgStr.StartsWith("猜错咯，") && auto1A2BDoingGroup?.GroupId == groupMessage.Group.GroupId)
            {
                if (msgStr.Contains('A') && msgStr.Contains('B'))
                {
                    await Task.Delay(2000).ConfigureAwait(false);
                    string abInfoStr = msgStr[(msgStr.IndexOf('A') - 1)..(msgStr.IndexOf('B') + 1)];
                    int aCount = int.Parse(abInfoStr.Substring(0, 1));
                    int bCount = int.Parse(abInfoStr.Substring(2, 1));
                    var possibilities = CaculateHelper.Game1A2B.Game1A2BGetAllPossibility(auto1A2BLastGuess!, aCount + bCount);
                    List<string> possibilitiesAsString = new();
                    foreach (var possibility in possibilities)
                    {
                        possibilitiesAsString.Add(string.Join("", possibility));
                    }
                    if (currentAuto1A2BHashSet.Count == 0)
                    {
                        currentAuto1A2BHashSet.UnionWith(possibilitiesAsString);
                    }
                    else
                    {
                        currentAuto1A2BHashSet.IntersectWith(possibilitiesAsString);
                        if (auto1A2BLastGuess != null)
                            currentAuto1A2BHashSet.ExceptWith(new string[] { string.Join("", auto1A2BLastGuess) });
                    }
                    List<int> setAsInt = new();
                    foreach (var possibility in currentAuto1A2BHashSet)
                    {
                        setAsInt.Add(int.Parse(string.Join("", possibility)));
                    }
                    setAsInt.Sort();
                    await msg.MessageWindow.SendMessageAsync($"我猜{setAsInt[0]:0000}").ConfigureAwait(false);
                    auto1A2BLastGuess = setAsInt[0].ToString("0000").ToArray();
                }
            }
        }
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
