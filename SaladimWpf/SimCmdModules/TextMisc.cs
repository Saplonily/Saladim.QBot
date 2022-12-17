using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using CodingSeb.ExpressionEvaluator;
using Microsoft.Extensions.DependencyInjection;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;
using SaladimQBot.SimCommand;
using SaladimWpf.Services;

namespace SaladimWpf.SimCmdModules;

public class TextMisc : CommandModule
{
    private readonly IServiceProvider serviceProvider;
    private readonly SalLoggerService salLoggerService;
    private readonly SaladimWpfService saladimWpfService;

    public TextMisc(SaladimWpfService saladimWpfService, IServiceProvider serviceProvider, SalLoggerService salLoggerService)
    {
        this.saladimWpfService = saladimWpfService;
        this.serviceProvider = serviceProvider;
        this.salLoggerService = salLoggerService;
    }

    [Command("hello")]
    public void Hello()
    {
        Content.MessageWindow.SendMessageAsync($"Hello SimCommand!!!, 这是本客户端的昵称: {saladimWpfService.Client.Self.Nickname}").Wait();
    }

    [Command("random")]
    public void Random()
    {
        Random r = serviceProvider.GetRequiredService<RandomService>().Random;
        int num = r.Next(0, 100);
        IMessageEntity e = Content.Client.CreateMessageBuilder()
            .WithAt(Content.Message.Sender)
            .WithText($"{Content.Executer.Nickname},你的随机数为{num}哦~")
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
        Debug.Assert(OperatingSystem.IsWindows());
        var bitmap = new Bitmap(50, 50);
        var graphics = Graphics.FromImage(bitmap);
        var color = GetRandomColor();
        graphics.DrawRectangle(new Pen(color, 256), new Rectangle(0, 0, 256, 256));
        if (!Directory.Exists("tempImages")) Directory.CreateDirectory("tempImages");
        var imgName = $"{DateTime.Now.Ticks - 638064687298838726L}.png";
        var fileName = $"tempImages\\{imgName}";
        bitmap.Save(fileName);
        IMessageEntity entity = Content.Client.CreateMessageBuilder()
            .WithImage("http://127.0.0.1:5702/?img_name=" + imgName)
            .WithTextLine($"\nRGB: {color.R},{color.G},{color.B}")
            .WithText($"HEX: #{color.R:X}{color.G:X}{color.B:X}")
            .Build();
        Content.MessageWindow.SendMessageAsync(entity);

        Color GetRandomColor()
        {
            Random r = serviceProvider.GetRequiredService<RandomService>().Random;
            return Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
        }
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
}
