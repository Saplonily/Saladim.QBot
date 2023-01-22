using Microsoft.Extensions.DependencyInjection;
using Saladim.Offbot.Services;
using SaladimQBot.Core;
using SaladimQBot.Extensions;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SysColor = System.Drawing.Color;
using System.Text;

namespace Saladim.Offbot.SimCmdModules;

public class DrawImageModule : CommandModule
{
    protected readonly IServiceProvider serviceProvider;

    public DrawImageModule(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
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
}
