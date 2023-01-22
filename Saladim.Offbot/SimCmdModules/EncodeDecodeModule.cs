using SaladimQBot.Core;
using System.Diagnostics;
using System.Text;
using SaladimQBot.Extensions;
using SaladimQBot.GoCqHttp;
using Microsoft.Extensions.DependencyInjection;
using Saladim.Offbot.Services;
using SaladimQBot.Shared;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Security.Cryptography;
using System.Buffers.Text;

namespace Saladim.Offbot.SimCmdModules;

public class EncodeDecodeModule : CommandModule
{
    protected readonly IServiceProvider serviceProvider;

    public EncodeDecodeModule(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    [Command("图片编码")]
    public void EncodePicture(string s)
    {
        string fileName = $"tempImages\\{DateTime.Now.Ticks}.png";
        PicEncode(s, fileName);
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
                string str = PicDecode(s);
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

    [Command("md5")]
    public void Md5(params string[] strs)
    {
        string tip = "";
        if (strs.Length >= 2)
        {
            tip = "(多个空格已转换为单空格, 如需包含多空格请使用半角双引号包围)";
        }
        string str = string.Join(' ', strs);
        byte[] result = MD5.HashData(Encoding.UTF8.GetBytes(str));
        string resultStr = string.Concat(result.Select(b => b.ToString("x").PadLeft(2, '0')));
        Content.MessageWindow.SendTextMessageAsync($"结果md5{tip}: {resultStr}");
    }

    [Command("base64encode")]
    public void Base64Encode(params string[] strs)
    {
        string tip = "";
        if (strs.Length >= 2)
        {
            tip = "(多个空格已转换为单空格, 如需包含多空格请使用半角双引号包围)";
        }
        string str = string.Join(' ', strs);
        string result = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        Content.MessageWindow.SendTextMessageAsync($"结果base64{tip}: {result}");
    }

    [Command("base64decode")]
    public void Base64Decode(string str)
    {
        var result = Convert.FromBase64String(str);
        Content.MessageWindow.SendTextMessageAsync($"base64解码结果: {Encoding.UTF8.GetString(result)}");
    }

    public static string PicDecode(Stream stream)
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

    public static void PicEncode(string text, string path)
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
