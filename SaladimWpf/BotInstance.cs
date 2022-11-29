using SaladimQBot.GoCqHttp;
using Saladim.SalLogger;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using CodingSeb.ExpressionEvaluator;
using SaladimQBot.Core;

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
                string nameFormat = string.Empty;
                if (await groupMsg.Sender.Card.ValueAsync != string.Empty)
                {
                    nameFormat = $"{await groupMsg.Sender.Card.ValueAsync}" +
                        $"({await groupMsg.Sender.Nickname.ValueAsync})";
                }
                else
                {
                    nameFormat = await groupMsg.Sender.Nickname.ValueAsync;
                }
                logger.LogInfo(
                    "WpfConsole", $"{groupMsg.Group.Name.Value}({groupMsg.Group.GroupId}) {nameFormat} 说: " +
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
                await message.MessageWindow.SendMessageAsync($"{message.Sender.CqAt} {message.Sender.Nickname.Value},你的随机数为{num}哦~");
            }

            commandStart = "/来点图";
            if (rawString.StartsWith(commandStart))
            {
                HttpClient client = new();
                var result = client.GetAsync("https://img.xjh.me/random_img.php?return=json").Result;
                StreamReader reader = new(result.Content.ReadAsStream());
                string s = reader.ReadToEnd();
                JsonDocument doc = JsonDocument.Parse(s);
                string imageUrl = "http:" + doc.RootElement.GetProperty("img").GetString()!;
                CqMessageEntity cqEntity = new()
                {
                    new CqMessageImageSendNode(imageUrl)
                };
                await message.MessageWindow.SendMessageAsync(new MessageEntity(cqEntity));
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
                }
            }

            commandStart = "/撤回";
            if (rawString.StartsWith(commandStart))
            {
                var replyNode =
                    (from node in message.MessageEntity.CqEntity
                     where node is CqMessageReplyIdNode
                     let replyIdNode = node as CqMessageReplyIdNode
                     select replyIdNode).First();
                if (replyNode is not null)
                {
                    _ = client.RecallMessageAsync(replyNode.MessageId);
                }
            }

            commandStart = "/狠狠骂我";
            if (rawString.StartsWith("狠狠骂我"))
            {
                var msg = await message.MessageWindow.SendMessageAsync("cnm, 有病吧");
                await Task.Delay(1000);
                await msg.RecallAsync();
                await message.MessageWindow.SendMessageAsync("qwq, 怎么能骂人呢awa");

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
