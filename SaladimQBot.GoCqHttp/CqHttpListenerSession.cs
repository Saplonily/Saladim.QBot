using System.Net;
using System.Text.Json;

namespace SaladimQBot.GoCqHttp;

public class CqHttpListenerSession : ICqSession
{
    protected HttpListener listener;
    protected Task? listenTask;

    public bool Started { get; protected set; }

    public event OnCqSessionReceivedHandler? OnReceived;

    /// <summary>
    /// 可接受异常发生时的通知事件
    /// </summary>
    public event Action<Exception>? OnReceivedAcceptableException;

    public CqHttpListenerSession(string goCqHttpBaseUrl)
    {
        listener = new HttpListener();
        listener.Prefixes.Add(goCqHttpBaseUrl + "/");
    }

    public Task<CqApiCallResult?> CallApiAsync(CqApi api)
        => throw new NotSupportedException("CqHttpListenerSession doesn't support CallApi.");

    public void Dispose()
    {
        listener.Stop();
        listener.Close();
    }

    public Task StartAsync()
    {
        Started = true;
        try
        {
            listener.Start();
            Task.Run(ListenLoop);
        }
        catch (Exception)
        {
            Started = false;
            throw;
        }
        return Task.CompletedTask;
    }

    protected void ListenLoop()
    {
        while (true)
        {
            var content = listener.GetContext();
            Stream stream = content.Request.InputStream;

            try
            {
                JsonDocument doc = JsonDocument.Parse(stream);
                OnReceived?.Invoke(doc);
            }
            catch (JsonException jsonException)
            {
                var e = new PostParseFailedException(jsonException);
                OnReceivedAcceptableException?.Invoke(e);
            }
            finally
            {
                content.Response.StatusCode = 200;
                content.Response.Close();
            }
        }
    }
}
