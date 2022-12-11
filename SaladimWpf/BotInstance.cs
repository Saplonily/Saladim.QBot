using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CodingSeb.ExpressionEvaluator;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;

namespace SaladimWpf;

public class BotInstance
{
    protected HttpClient httpClient;
    protected CqClient cqClient;
    protected Logger logger;

    public event Action<string>? OnClientLog;

    public event Action<string>? OnLog;

    public bool OpenGuessNumberBot { get; set; }

    public int GuessNumberBotDelay { get; set; }

    public BotInstance(string address)
    {
        cqClient = new CqWebSocketClient(address, LogLevel.Trace);
        cqClient.OnLog += s => OnClientLog?.Invoke(s);
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Saladim.QBot external value fetcher");
        logger = new LoggerBuilder()
            .WithLevelLimit(LogLevel.Trace)
            .WithAction(s => OnLog?.Invoke(s))
            .Build();

        cqClient.OnMessageReceived += Client_OnMessageReceived;
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

            #region /random
            commandStart = "/random";
            if (rawString.StartsWith(commandStart))
            {
                Random r = new();
                int num = r.Next(0, 100);
                string s = $"{message.Sender.CqAt} {message.Sender.Nickname.Value},你的随机数为{num}哦~";
                await message.MessageWindow.SendMessageAsync(s);
            }
            #endregion

            #region /算
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
                    string s = $"错误发生了, 以下是错误信息:\n{e.Message}";
                    await message.MessageWindow.SendMessageAsync(s);
                }
            }
            #endregion

            #region /狠狠骂我
            commandStart = "/狠狠骂我";
            if (rawString.StartsWith(commandStart))
            {
                var msg = await message.MessageWindow.SendMessageAsync("cnm, 有病吧");
                await Task.Delay(2000);
                await msg.RecallAsync();
                await Task.Delay(2000);
                await message.MessageWindow.SendMessageAsync("qwq我刚才肯定没有骂人");

            }
            #endregion

            #region /来点图
            commandStart = "/来点图";
            if (rawString.StartsWith(commandStart))
            {
                string s = await httpClient.GetStringAsync("https://img.xjh.me/random_img.php?return=json");
                string imgUrl = "http:" + JsonDocument.Parse(s).RootElement.GetProperty("img").GetString();
                MessageEntity m = new MessageEntityBuilder(cqClient)
                    .WithImage(imgUrl)
                    .Build();
                await message.MessageWindow.SendMessageAsync(m);
            }
            #endregion

            #region /视频信息
            commandStart = "/视频信息 ";
            if (rawString.StartsWith(commandStart))
            {
                string query = "";
                string input = rawString[commandStart.Length..];
                if (input.StartsWith("BV"))
                    query = "bvid=" + input;
                else if (long.TryParse(input, out _))
                    query = "aid=" + input;
                else
                {
                    await message.MessageWindow.SendMessageAsync(
                        "输入格式不正确, 已取消本次api请求.\n" +
                        "可接受的格式: BV开头的bv号(如:BV17x411w7KC), 纯数字的av号(如:170001)"
                        );
                    return;
                }
                string url = $"https://api.bilibili.com/x/web-interface/view?{query}";
                var d =
                    JsonSerializer.Deserialize<Models.BilibiliVideoInfoApiCallResult.Root>
                        (await httpClient.GetStringAsync(url));
                if (d is null)
                    return;
                long code = d.Code;
                string? errMsg = d.Message;
                if (code == 0)
                {
                    StringBuilder sb = new();
                    sb.AppendLine("视频信息如下: ");
                    sb.AppendLine($"av号: av{d.Data.Aid}, bv号: {d.Data.Bvid}");
                    sb.AppendLine($"up主: {d.Data.Owner.Name}(uid{d.Data.Owner.Mid})");
                    sb.AppendLine($"视频标题: {d.Data.Title}");
                    sb.AppendLine($"分区: {d.Data.Tname}");
                    sb.AppendLine($"发布时间: {DateTimeHelper.GetFromUnix(d.Data.Pubdate)}");
                    sb.AppendLine($"播放: {d.Data.Stat.View:###,###}, 弹幕: {d.Data.Stat.Danmaku:###,###}");
                    sb.AppendLine($"视频封面地址: {d.Data.Pic}");
                    sb.AppendLine($"视频长度: {TimeSpan.FromSeconds(d.Data.Duration)}");
                    sb.AppendLine($"弹幕cid: {d.Data.Cid}");
                    if (d.Data.Videos != 1)
                        sb.AppendLine($"视频分p数: {d.Data.Videos}");
                    sb.Append($"点赞: {d.Data.Stat.Like:###,###}, 投币: {d.Data.Stat.Coin:###,###}, ");
                    sb.AppendLine($"收藏: {d.Data.Stat.Favorite:###,###}, 分享: {d.Data.Stat.Share}");
                    sb.AppendLine($"版权信息: {(d.Data.Copyright == 1 ? "原创" : "转载")}");
                    sb.AppendLine($"动态信息: {d.Data.Dynamic}");
                    sb.AppendLine($"视频简介: {d.Data.Desc.Replace("\n", " {n} ")}");
                    await message.MessageWindow.SendMessageAsync(sb.ToString());
                }
                else
                {
                    await message.MessageWindow.SendMessageAsync($"错误发生了, 错误代码:{code}, 错误信息:\n{errMsg}");
                }
            }
            #endregion

            #region auto猜数游戏
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
            #endregion
        }
    }

    public async Task StartAsync()
    {
        await cqClient.StartAsync();
    }

    public async Task StopAsync()
    {
        await cqClient.StopAsync();
    }
}
