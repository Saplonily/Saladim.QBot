using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodingSeb.ExpressionEvaluator;
using Microsoft.Extensions.DependencyInjection;
using Saladim.Offbot.Services;
using SaladimQBot.Core;
using SaladimQBot.Extensions;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SysColor = System.Drawing.Color;

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

    [Command("gc_collect")]
    public void GCCollect()
    {
        string before = NumberHelper.GetSizeString(Process.GetCurrentProcess().PagedMemorySize64);
        Console.Clear();
        GC.Collect(0);
        GC.Collect(1);
        GC.Collect(2);
        string after = NumberHelper.GetSizeString(Process.GetCurrentProcess().PagedMemorySize64);
        Content.MessageWindow.SendTextMessageAsync($"已完成0,1,2三代清理工作");
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

    [Command("随机色块")]
    public void RandomPicColor(int size)
    {
        if (size > 1920)
        {
            Content.MessageWindow.SendTextMessageAsync("你这大小怎么大于1920了都, 想爆bot内存是吧(((");
            return;
        }
        Random r = serviceProvider.GetRequiredService<RandomService>().Random;
        Image<Rgba32> image = new(size, size);
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                image[i, j] = new Color(new Rgba32((byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256)));
            }
        if (!Directory.Exists("tempImages"))
            Directory.CreateDirectory("tempImages");
        string fileName = $@"tempImages\{DateTime.Now.Ticks}.png";
        image.SaveAsPng(fileName);
        IMessageEntity entity = Content.Client.CreateMessageBuilder().WithImage(new Uri(Path.GetFullPath(fileName))).Build();
        Content.MessageWindow.SendMessageAsync(entity);
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

    [Command("来点颜色")]
    public void GetSomeColor()
    {
        var color = GetRandomColor();
        Uri fileUri = HappyDrawing(50, 50, process =>
        {
            SolidBrush brush = new(color.ToISColor());
            process.Fill(brush, new RectangleF(0.0f, 0.0f, 50.0f, 50.0f));
        });
        IMessageEntity entity = Content.Client.CreateMessageBuilder()
            .WithImage(fileUri)
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
        Uri uri = HappyDrawing((int)width, (int)height, process =>
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
        IMessageEntityBuilder builder = Content.Client.CreateMessageBuilder().WithImage(uri);
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

    public static Uri HappyDrawing(int width, int height, Action<IImageProcessingContext> processContext)
    {
        Image<Rgba32> image = new(width, height);
        image.Mutate(processContext);
        if (!Directory.Exists("tempImages")) Directory.CreateDirectory("tempImages");
        var imgName = $"{DateTime.Now.Ticks}.png";
        var fileName = $@"tempImages\{imgName}";
        image.SaveAsPng(fileName);
        return new Uri(Path.GetFullPath(fileName));
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

    [Command("图片编码")]
    public void EncodePicture(string s)
    {
        string fileName = $"tempImages\\{DateTime.Now.Ticks}.png";
        Encode(s, fileName);
        IMessageEntity entity = Content.Client.CreateMessageBuilder()
            .WithTextLine("编码成功, 以下是图片:")
            .WithImage(new Uri(Path.GetFullPath(fileName)))
            .Build();
        Content.MessageWindow.SendMessageAsync(entity);
    }

    [Command("图片解码")]
    public void DecodePicture()
    {
        var image = Content.Message.MessageEntity.FirstImageOrNull();
        if (image is null)
        {
            Content.MessageWindow.SendTextMessageAsync("请输入一张图片");
            return;
        }
        try
        {
            if (image.FileUri.Scheme is ("http" or "https"))
            {
                HttpClient httpClient = serviceProvider.GetRequiredService<HttpRequesterService>().HttpClient;
                string fileName = $"tempImages\\{DateTime.Now.Ticks}";
                Stream s = httpClient.GetStreamAsync(image.FileUri).GetResultOfAwaiter();
                string str = Decode(s);
                Content.MessageWindow.SendTextMessageAsync($"解码成功, 内容如下\n{str}");
            }
            else
            {
                Content.MessageWindow.SendTextMessageAsync($"内部错误, 图片uri期望的scheme:http, https, 实际: {image.FileUri.Scheme}");
            }
        }
        catch
        {
            Content.MessageWindow.SendTextMessageAsync("解码失败, 可能原因: 图片不是正方形、utf8解码失败");
        }
    }

    public static string Decode(Stream stream)
    {
        using Image rawImage = Image.Load(stream);
        using Image<Rgba32> image = rawImage.CloneAs<Rgba32>();
        int width = image.Width == image.Height ? image.Width : throw new Exception("no equal width and height");
        int pixelCounts = width * width;
        Rgba32[] pixels = new Rgba32[pixelCounts];
        pixels.Initialize();
        for (int i = 0; i < width; i++)
            for (int j = 0; j < width; j++)
            {
                pixels[i + j * width] = image[i, j];
            }
        byte[] bytes = new byte[pixelCounts * 4];
        for (int i = 0; i < pixelCounts; i++)
        {
            bytes[i * 4] = pixels[i].R;
            bytes[i * 4 + 1] = pixels[i].G;
            bytes[i * 4 + 2] = pixels[i].B;
            bytes[i * 4 + 3] = pixels[i].A;
        }
        string s = Encoding.UTF8.GetString(bytes);
        s = s.Replace("\0", "");
        return s;
    }

    public static void Encode(string text, string path)
    {
        int bytesCount = Encoding.UTF8.GetByteCount(text);
        int bytesAlineWith4 = (int)Math.Ceiling((double)bytesCount / 4) * 4;
        Span<byte> bytes = new(new byte[bytesAlineWith4]);
        Encoding.UTF8.GetBytes(text.AsSpan(), bytes);

        int width = (int)Math.Ceiling(Math.Sqrt(bytesAlineWith4 / 4));
        int pixelsCount = bytesAlineWith4 / 4;
        Rgba32[] pixels = new Rgba32[pixelsCount];
        for (int i = 0; i < pixelsCount; i++)
        {
            pixels[i] = new Rgba32(bytes[i * 4], bytes[i * 4 + 1], bytes[i * 4 + 2], bytes[i * 4 + 3]);
        }

        using Image<Rgba32> image = new(width, width);
        for (int i = 0; i < width; i++)
            for (int j = 0; j < width; j++)
            {
                try
                {
                    image[i, j] = pixels[i + j * width];
                }
                catch
                {
                    image[i, j] = new Rgba32(0, 0, 0, 0);
                }
            }
        using FileStream fs = new(path, FileMode.Create, FileAccess.Write);
        image.SaveAsPng(fs);
    }
}