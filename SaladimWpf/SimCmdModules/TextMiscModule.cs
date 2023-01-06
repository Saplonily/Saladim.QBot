using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodingSeb.ExpressionEvaluator;
using Microsoft.Extensions.DependencyInjection;
using SaladimQBot.Core;
using SaladimQBot.Extensions;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;
using SaladimWpf.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SQLite;
using SysColor = System.Drawing.Color;

namespace SaladimWpf.SimCmdModules;

public partial class TextMiscModule : CommandModule
{
    private readonly IServiceProvider serviceProvider;
    private readonly SalLoggerService salLoggerService;
    private readonly SessionSqliteService sessionService;

    public TextMiscModule(IServiceProvider serviceProvider, SalLoggerService salLoggerService, SessionSqliteService sessionService)
    {
        this.serviceProvider = serviceProvider;
        this.salLoggerService = salLoggerService;
        this.sessionService = sessionService;
    }

    [Table("test_session")]
    public class TestSession : SqliteStoreSession
    {
        [Column("test_num")]
        public int Num { get; set; }
    }

    [Command("check_env")]
    public void CheckEnv()
    {
        var os = Environment.OSVersion;
        StringBuilder sb = new();
        sb.AppendLine($"运行平台: {os.Platform}");
#if DEBUG
        sb.AppendLine($"环境: DEBUG");
#else
        sb.AppendLine($"环境: RELEASE");
#endif
        sb.AppendLine($"运行程序: {Process.GetCurrentProcess().ProcessName}");
        sb.AppendLine($".NET clr版本: {Environment.Version}");
        sb.AppendLine($"程序内存占用: {NumberHelper.GetSizeString(Process.GetCurrentProcess().PagedMemorySize64)}");
        Content.MessageWindow.SendMessageAsync(sb.ToString());
    }

    [Command("add_my")]
    public void AddMy()
    {
        var s = sessionService.GetUserSession<TestSession>(Content.Executor.UserId);
        s.Num += 1;
        sessionService.SaveSession(s);
        Content.MessageWindow.SendMessageAsync(Content.Client.CreateTextOnlyEntity($"已为您的session.Num增加1, 目前值: {s.Num}"));
    }

    [Command("random")]
    public void Random(int min, int max)
    {
        Random r = serviceProvider.GetRequiredService<RandomService>().Random;
        int num = r.Next(min, max);
        IMessageEntity e = Content.Client.CreateMessageBuilder()
            .WithAt(Content.Message.Sender)
            .WithText($"{Content.Executor.Nickname},你的随机数为{num}哦~")
            .Build();
        Content.MessageWindow.SendMessageAsync(e);
    }

    [Command("算")]
    public void Calculate(string what)
    {
        try
        {
            ExpressionEvaluator e = new();
            string rst = e.Evaluate(what)?.ToString() ?? "";
            Content.MessageWindow.SendMessageAsync($"计算结果是:\n{rst}");
        }
        catch (Exception e)
        {
            salLoggerService.SalIns.LogWarn("Program", "Calculate", e);
            string s = $"错误发生了, 以下是错误信息:\n{e.Message}";
            Content.MessageWindow.SendMessageAsync(s);
        }
    }

    [Command("来点图")]
    public void GetSomePicture()
    {
        var httpClient = serviceProvider.GetRequiredService<HttpRequesterService>().HttpClient;
        string s = httpClient.GetStringAsync("https://img.xjh.me/random_img.php?return=json").GetResultOfAwaiter();
        string imgUrl = "http:" + JsonDocument.Parse(s).RootElement.GetProperty("img").GetString();
        IMessageEntity m = Content.Client.CreateMessageBuilder()
            .WithImage(imgUrl)
            .Build();
        Content.MessageWindow.SendMessageAsync(m);
    }

    [Command("视频信息")]
    public void VideoInfo(string videoIdInput)
    {
        string query;
        if (videoIdInput.StartsWith("BV"))
            query = "bvid=" + videoIdInput;
        else if (long.TryParse(videoIdInput, out _))
            query = "aid=" + videoIdInput;
        else
        {
            Content.MessageWindow.SendMessageAsync(
                "输入格式不正确, 已取消本次api请求.\n" +
                "可接受的格式: BV开头的bv号(如:BV17x411w7KC), 纯数字的av号(如:170001)"
                );
            return;
        }
        string url = $"https://api.bilibili.com/x/web-interface/view?{query}";
        var httpClient = serviceProvider.GetRequiredService<HttpRequesterService>().HttpClient;
        var d = JsonSerializer.Deserialize<Models.BilibiliVideoInfoApiCallResult.Root>(httpClient.GetStringAsync(url).GetResultOfAwaiter());
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
            Content.MessageWindow.SendMessageAsync(sb.ToString());
        }
        else
        {
            Content.MessageWindow.SendMessageAsync($"错误发生了, 错误代码:{code}, 错误信息:\n{errMsg}");
        }
    }

    [Command("来点颜色")]
    public void GetSomeColor()
    {
        var color = GetRandomColor();
        string url = HappyDrawing(50, 50, process =>
        {
            SolidBrush brush = new(color.ToISColor());
            process.Fill(brush, new RectangleF(0.0f, 0.0f, 50.0f, 50.0f));
        });
        IMessageEntity entity = Content.Client.CreateMessageBuilder()
            .WithImage(url)
            .WithTextLine(GetColorText(color))
            .Build();
        Content.MessageWindow.SendMessageAsync(entity);

        SysColor GetRandomColor()
        {
            Random r = serviceProvider.GetRequiredService<RandomService>().Random;
            return SysColor.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
        }
    }

    [Command("做旗子")]
    public void MakeFlagInternal(params SysColor[] colors)
    {
        var pointsCount = colors.Length;
        float width = 576; float height = 384;
        string url = HappyDrawing((int)width, (int)height, process =>
        {
            var partWidth = width / pointsCount;
            var solidBrushes = new SolidBrush[pointsCount];
            for (int i = 0; i < pointsCount; i++)
            {
                solidBrushes[i] = new SolidBrush(colors[i].ToISColor());
                var curLeftTop = new PointF(partWidth * i, 0.0f);
                process.Fill(solidBrushes[i], new RectangleF(curLeftTop, new SizeF(partWidth, height)));
            }
        });
        IMessageEntityBuilder builder = Content.Client.CreateMessageBuilder().WithImage(url);
        StringBuilder sb = new();
        foreach (var color in colors)
        {
            sb.Append(GetColorText(color));
            sb.Append(" | ");
        }
        string s = sb.ToString();
        if (s.Length > 250)
        {
            s = "颜色信息长于250字符, 已默认屏蔽.";
        }
        builder.WithTextLine(s);
        Content.MessageWindow.SendMessageAsync(builder.Build());
    }

    public static string GetColorText(SysColor color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}({color.R},{color.G},{color.B})";

    public static string HappyDrawing(int width, int height, Action<IImageProcessingContext> processContext)
    {
        Image<Rgba32> image = new(width, height);
        image.Mutate(processContext);
        if (!Directory.Exists("tempImages")) Directory.CreateDirectory("tempImages");
        var imgName = $"{DateTime.Now.Ticks}.png";
        var fileName = $@"tempImages\{imgName}";
        image.SaveAsPng(fileName);
        return $"file:///{Path.GetFullPath(fileName)}";
    }

    [Command("homo")]
    public void Homo(double num)
    {
        var jsService = serviceProvider.GetRequiredService<JavaScriptService>();
        string rst = jsService.Homo(num);
        string output = $"恶臭后的 {num} = {rst}";
        if (rst.Length <= 100)
        {
            Content.MessageWindow.SendMessageAsync(output);
        }
        else
        {
            IForwardEntity f = Content.Client.CreateForwardBuilder()
                .AddMessage(Content.Client.CreateTextOnlyEntity(output))
                .AddMessage(Content.Client.CreateTextOnlyEntity("内容长度过长, 已转换至群体转发消息."))
                .Build();
            Content.MessageWindow.SendMessageAsync(f);
        }
    }

    public readonly string[] IgnoreWords = new string[]
    {
        "禁言", "傻逼", "智障", "煞笔", "我是", "你是"
    };

    [GeneratedRegex("echo")]
    private static partial Regex EchoCountRegex();

    [Command("echo")]
    public void Echo(params string[] s)
    {
        string str = string.Join(' ', s);
        if (IgnoreWords.Any(str.Contains))
        {
            return;
        }
        Regex r = EchoCountRegex();

        if (r.Matches(str).Count >= 3)
        {
            if (Content.Message is IGroupMessage groupMessage)
            {
                groupMessage.Sender.BanAsync(TimeSpan.FromMinutes(1));
            }
            return;
        }
        Content.MessageWindow.SendMessageAsync(string.Join("​", str.ToCharArray()));
    }

    [Command("不定积分")]
    public async void Integral(params string[] strs)
    {
        string s = string.Join(' ', strs);
        var service = serviceProvider.GetRequiredService<IntegralCalculatorService>();
        var result = await service.IntegralOf(s).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(result))
        {
            await Content.MessageWindow.SendMessageAsync(result).ConfigureAwait(false);
        }
        else
            await Content.MessageWindow.SendMessageAsync("计算错误").ConfigureAwait(false);
    }

    [Command("my_avatar")]
    public void MyAvatar()
    {
        var entity = Content.Client.CreateMessageBuilder().WithTextLine("你的头像:").WithImage(Content.Executor.AvatarUrl).Build();
        Content.MessageWindow.SendMessageAsync(entity);
    }

    [Command("group_avatar")]
    public void GroupAvatar()
    {
        if (Content.Message is IGroupMessage groupMessage)
        {
            var entity = Content.Client.CreateMessageBuilder().WithTextLine("群头像:").WithImage(groupMessage.Group.AvatarUrl).Build();
            Content.MessageWindow.SendMessageAsync(entity);
        }
    }

    [Command("tts")]
    public void TTS(string tts)
    {
        var ttsParam = new Dictionary<string, string>()
        {
            ["text"] = tts
        };
        Content.MessageWindow.SendMessageAsync(Content.Client.CreateMessageBuilder().WithUnImpl("tts", ttsParam).Build());
    }

    [Command("new_co")]
    public void NewCo()
    {
        serviceProvider.GetRequiredService<CoroutineService>().AddNewCoroutine(Co());
    }

    public IEnumerator<EventWaiter> Co()
    {
        Content.MessageWindow.SendMessageAsync("协程开始了");
        yield return new CommandWaiter(Content.SimCommandExecuter, Content.Executor, "push1");
        Content.MessageWindow.SendMessageAsync("协程被推动了push1");

        yield return new CommandWaiter(Content.SimCommandExecuter, Content.Executor, "push4");
        Content.MessageWindow.SendMessageAsync("协程被推动了push4, 协程结束了");

        if (Content.Executor is IGroupUser groupUser)
        {
            IGroupMessage groupMsg = null!;
            yield return new MessageWaiter(groupUser, m => groupMsg = m);
            Content.MessageWindow.SendMessageAsync($"你之后说了{groupMsg.MessageEntity.RawString}哦~");
        }

        yield break;
    }

    [Command("push1")] public void Push1() { }
    [Command("push2")] public void Push2() { }
    [Command("push3")] public void Push3() { }
    [Command("push4")] public void Push4() { }
}
