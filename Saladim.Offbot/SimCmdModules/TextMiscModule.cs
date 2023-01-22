using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodingSeb.ExpressionEvaluator;
using Microsoft.Extensions.DependencyInjection;
using Saladim.Offbot.Services;
using SaladimQBot.Core;
using SaladimQBot.Extensions;
using SaladimQBot.Shared;

namespace Saladim.Offbot.SimCmdModules;

public partial class TextMiscModule : CommandModule
{
    private readonly IServiceProvider serviceProvider;
    private readonly SalLoggerService salLoggerService;

    public TextMiscModule(IServiceProvider serviceProvider, SalLoggerService salLoggerService)
    {
        this.serviceProvider = serviceProvider;
        this.salLoggerService = salLoggerService;
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
            .WithImage(new Uri(imgUrl))
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
        Content.MessageWindow.SendMessageAsync(string.Join((char)8203, str.ToCharArray()));
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

    [Command("homo")]
    public void Homo(double num)
    {
        var homoService = serviceProvider.GetRequiredService<HomoService>();
        string text = $"恶臭后的{num} = {homoService.Homo(num)}";
        IMessageEntity entity = Content.Client.CreateTextOnlyEntity(text);
        if (text.Length >= 150)
        {
            IForwardEntity forwardEntity = Content.Client.CreateForwardBuilder().AddMessage(entity).Build();
            Content.MessageWindow.SendMessageAsync(forwardEntity);
        }
        else
        {
            Content.MessageWindow.SendMessageAsync(entity);
        }
    }
}