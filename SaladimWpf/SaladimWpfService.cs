using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.Core.Services;
using SaladimQBot.GoCqHttp;
using SaladimQBot.SimCommand;
using SaladimWpf.Services;

namespace SaladimWpf;

public class SaladimWpfService : IClientService
{
    protected Logger logger;
    protected CqWebSocketClient wsClient;
    protected SimCommandService simCommandService;

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
        this.logger = loggerService.SalIns;
        wsClient.OnLog += s => logger.LogInfo("WsClient", s);
        this.simCommandService = simCommandService;
        Client.OnMessageReceived += this.Client_OnMessageReceived;
    }

    private async void Client_OnMessageReceived(IMessage message)
    {
        #region log
        if (message is GroupMessage groupMsg)
        {
            logger.LogInfo(
                "WpfConsole", $"{groupMsg.Group.Name.Value}({groupMsg.Group.GroupId}) - " +
                $"{groupMsg.Author.FullName} 说: " +
                $"{groupMsg.MessageEntity.RawString}"
                );
#if DEBUG
            if (groupMsg.Group.GroupId != 860355679)
                return;
#endif
        }
        else if (message is PrivateMessage privateMsg)
        {
            logger.LogInfo(
                "WpfConsole", $"{await privateMsg.Sender.Nickname.ValueAsync.ConfigureAwait(false)}" +
                $"({privateMsg.Sender.UserId}) 私聊你: {privateMsg.MessageEntity.RawString}"
                );
        }

        //重要, 框架不支持临时消息
        if (message is not (GroupMessage or FriendMessage))
            return;

        #endregion
        //sim command!
        await simCommandService.Executor.MatchAndExecuteAllAsync(message).ConfigureAwait(false);
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
