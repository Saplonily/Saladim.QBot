using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Hosting;
using Saladim.SalLogger;

namespace SaladimWpf.Services;

public class HttpServerService
{
    protected const string ListenOn = "http://127.0.0.1:5702/";
    protected HttpListener httpListener;
    protected Logger logger;
    protected CancellationTokenSource cancellationTokenSource;

    public HttpServerService(SalLoggerService salLoggerService, IHostApplicationLifetime hostApplicationLifetime)
    {
        logger = salLoggerService.SalIns;
        httpListener = new();
        httpListener.Prefixes.Add(ListenOn);
        cancellationTokenSource = new();
        hostApplicationLifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                httpListener.Start();
            }
            catch (Exception e)
            {
                logger.LogError(nameof(HttpServerService), e, $"Failed to start listen on {ListenOn}");
                cancellationTokenSource.Cancel();
            }
            Task.Run(() => ListenerLoop(cancellationTokenSource.Token), cancellationTokenSource.Token);
        });
        hostApplicationLifetime.ApplicationStopping.Register(() =>
        {
            cancellationTokenSource.Cancel();
            try
            {
                httpListener.Stop();
            }
            catch (ObjectDisposedException) { }
        });
    }

    private void ListenerLoop(CancellationToken token)
    {
        try
        {
            while (true && !token.IsCancellationRequested)
            {
                var context = httpListener.GetContext(); //监听http请求

                var imgName = context.Request.QueryString["img_name"]; //获取query string
                var fileName = $"tempImages\\{imgName}";
                if (File.Exists(fileName))
                {
                    using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
                    var res = context.Response;
                    res.ContentType = "image/bmp";
                    res.StatusCode = 200;//状态码
                    CopyStream(fs, context.Response.OutputStream);//写入返回流
                    res.Close();//完成回应
                    continue;
                }
                else
                {
                    //图片
                    var res = context.Response;
                    res.ContentType = "text/plain";
                    res.StatusCode = 404;
                    using StreamWriter sw = new(context.Response.OutputStream, Encoding.UTF8);
                    sw.WriteLine("图片未找到");
                    sw.Close();
                    context.Response.Close();
                    continue;
                }
            }
        }
        catch (HttpListenerException)
        {

        }
    }
    public static void CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[3 * SaladimQBot.Shared.Size.KiB];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }
}
