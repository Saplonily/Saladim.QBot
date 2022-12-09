using CodingSeb.ExpressionEvaluator;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;

namespace SaladimWpf;

public class BotInstance
{
    protected CqClient client;
    protected Logger logger;

    public event Action<string>? OnClientLog;

    public event Action<string>? OnLog;

    public bool OpenGuessNumberBot { get; set; }
    public int GuessNumberBotDelay { get; set; }

    public BotInstance(string address)
    {
        client = new CqWebSocketClient(address, LogLevel.Trace);
        client.OnLog += s => OnClientLog?.Invoke(s);
        logger = new LoggerBuilder()
            .WithLevelLimit(LogLevel.Trace)
            .WithAction(s => OnLog?.Invoke(s))
            .Build();

        client.OnMessageReceived += Client_OnMessageReceived;
    }

    private async void Client_OnMessageReceived(Message message)
    {
        await Task.Run(OnMessageReceived);
        async void OnMessageReceived()
        {
            if (message is GroupMessage groupMsg)
            {
                logger.LogInfo(
                    "WpfConsole", $"{groupMsg.Group.Name.Value}({groupMsg.Group.GroupId}) - " +
                    $"{groupMsg.Author.FullName} 说: " +
                    $"{groupMsg.MessageEntity.RawString}"
                    );
            }
            else if (message is PrivateMessage privateMsg)
            {
                logger.LogInfo(
                    "WpfConsole", $"{await privateMsg.Sender.Nickname.ValueAsync}" +
                    $"({privateMsg.Sender.UserId}) 私聊你: {privateMsg.MessageEntity.RawString}"
                    );
            }

            string rawString = message.MessageEntity.RawString.Trim();
            string commandStart = string.Empty;

            commandStart = "/random";
            if (rawString.StartsWith(commandStart))
            {
                Random r = new();
                int num = r.Next(0, 100);
                string s = $"{message.Sender.CqAt} {message.Sender.Nickname.Value},你的随机数为{num}哦~";
                await message.MessageWindow.SendMessageAsync(s);
            }

            commandStart = "/算 ";
            if (rawString.StartsWith(commandStart))
            {
                try
                {
                    string thing = rawString[commandStart.Length..];
                    ExpressionEvaluator e = new();
                    string rst = e.Evaluate(thing)?.ToString() ?? "";
                    await message.MessageWindow.SendMessageAsync($"计算结果是: {rst}");
                }
                catch (Exception e)
                {
                    logger.LogWarn("Program", "Calculate", e);
                    string s = $"错误发生了, 以下是错误信息:\n{e.Message}\n{e.StackTrace}";
                    await message.MessageWindow.SendMessageAsync(s);
                }
            }

            commandStart = "/狠狠骂我";
            if (rawString.StartsWith(commandStart))
            {
                var msg = await message.MessageWindow.SendMessageAsync("cnm, 有病吧");
                await Task.Delay(2000);
                await msg.RecallAsync();
                await Task.Delay(2000);
                await message.MessageWindow.SendMessageAsync("qwq我刚才肯定没有骂人");

            }

            //auto猜数游戏
            if (OpenGuessNumberBot && rawString.Contains("您猜了") && rawString.Contains("但是猜的数"))
            {
                const string currentRegion = "当前范围:";
                int loc = rawString.IndexOf(currentRegion);
                if (loc is not -1)
                {
                    string regionString = rawString[(loc + currentRegion.Length)..];
                    string[] regions = regionString.Split("~");
                    long num1 = long.Parse(regions[0]);
                    long num2 = long.Parse(regions[1]);
                    long target = (num1 + num2) / 2;
                    logger.LogInfo("WpfConsole", "猜数", $"区域字符串为: {regionString}, 计算结果: {target}");
                    await Task.Delay(this.GuessNumberBotDelay);
                    await message.MessageWindow.SendMessageAsync($"猜{target}");
                }
            }
        }
    }

    public async Task StartAsync()
    {
        await client.StartAsync();
    }

    public async Task StopAsync()
    {
        await client.StopAsync();
    }
}
