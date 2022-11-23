using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public delegate void OnCqSessionReceivedHandler(in JsonDocument parsedDocment);

public sealed class CqWebSocketSession : ICqSession, IDisposable
{
    public const int BufferSize = 1 * Size.MiB;
    public static readonly Encoding Encoding = Encoding.UTF8;
    public readonly Uri Uri;
    private ClientWebSocket webSocket = new();
    private CancellationTokenSource receiveTokenSource = new();
    private readonly Dictionary<string, Action<JsonDocument>> suspendedApiResponseWaitings = new();

    internal class PostParseFailedException : Exception
    {
        public PostParseFailedException(Exception? innerException = null) : base(string.Empty, innerException)
        {
        }
    }

    /// <summary>
    /// 是否使用事件端口
    /// </summary>
    public bool UseEventEndPoint { get; private set; } = false;

    /// <summary>
    /// 是否使用api端口
    /// </summary>
    public bool UseApiEndPoint { get; private set; } = false;

    /// <summary>
    /// true: 接收过程遇到可接受异常时中断连接
    /// false: 接受过程中遇到可接受异常时不中断连接
    /// 可接受异常: 指json解析出错异常
    /// 可接受异常会通过<see cref="OnReceivedAcceptableException"/>事件通知
    /// </summary>
    public bool AbortOnAcceptableException { get; private set; } = false;

    public Task? ReceivingTask { get; private set; }

    public bool Receiving { get; private set; }

    public WebSocketState State { get => webSocket.State; }


    /// <summary>
    /// 收到上报时引发的事件,注意Cancel/Abort/Stop后会主动清空事件的订阅者
    /// </summary>
    public event OnCqSessionReceivedHandler? OnReceived;

    /// <summary>
    /// 可接受异常发生时的通知事件
    /// </summary>
    public event Action<Exception>? OnReceivedAcceptableException;

    public CqWebSocketSession(string address, string? endpoint = null)
    {
        Uri = new($"ws://{address}/{endpoint}");
    }

    public CqWebSocketSession(string address, string? endpoint = null,
        bool useEventEndPoint = false, bool useApiEndPoint = false, bool abortOnAcceptableException = false)
        : this(address, endpoint) =>
        (UseEventEndPoint, UseApiEndPoint, AbortOnAcceptableException) =
        (useEventEndPoint, useApiEndPoint, abortOnAcceptableException);

    public void Dispose()
        => webSocket.Dispose();

    public Task ConnectAsync(CancellationToken token)
        => webSocket.ConnectAsync(Uri, token);

    public void Abort()
        => webSocket.Abort();

    public void RenewWebSocket()
    {
        if (webSocket.State is WebSocketState.Aborted or WebSocketState.Closed)
        {
            webSocket.Abort();
            webSocket.Dispose();
            webSocket = new();
            this.Receiving = false;
            this.ReceivingTask = null;
        }
    }

    public void EmitOnReceived(JsonDocument docToEmit)
    {
        if (this.UseEventEndPoint)
        {
            OnReceived?.Invoke(docToEmit);
        }
        if (this.UseApiEndPoint && docToEmit.RootElement.TryGetProperty(StringConsts.ApiEchoProperty, out var echoProp))
        {
            try
            {
                string aimEcho = echoProp.GetString() ?? string.Empty;
                if (suspendedApiResponseWaitings.TryGetValue(aimEcho, out var action))
                {
                    suspendedApiResponseWaitings.Remove(aimEcho);
                    action.Invoke(docToEmit);
                }

            }
            catch (InvalidOperationException) { } //忽略echo非string类型获值产生的异常
        }
    }

    public Task StartReceiving()
        => ReceivingTask = Task.Run(ReceivingLoop);

    public void ReceivingLoop()
    {
        var segment = GetArraySegment(BufferSize);
        try
        {
            Receiving = true;
            while (true)
            {
                int count = 0;
                WebSocketReceiveResult result;
                do
                {
                    result = webSocket.ReceiveAsync(segment, receiveTokenSource.Token).Result;
                    count += result.Count;
                }
                while (!result.EndOfMessage);
                //懒,出bug后再检测result的一些状态
                //2022-11-10 22:36:25 出bug了...数据量太大了
                string str = Encoding.GetString(segment.Array!, segment.Offset, count);
                try
                {
                    var doc = JsonDocument.Parse(str);
                    this.EmitOnReceived(doc);
                }
                catch (JsonException jsonException)
                {
                    var e = new PostParseFailedException(jsonException);
                    OnReceivedAcceptableException?.Invoke(e);
                }
            }
        }
        catch (AggregateException e)
        {
            if (e.InnerException is not TaskCanceledException)
                throw e;
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        finally
        {
            ReceivingTask = null;
            Receiving = false;
            webSocket.Abort();
            webSocket.Dispose();
        }
    }

    public void StopReceiving()
    {
        receiveTokenSource.Cancel();
        //2022-11-2 22:48:04,这个source cancel后状态一直保留着...
        //然后导致两个任务都无法继续...
        //我sb了
        receiveTokenSource = new();
        OnReceived = null;
    }

    /// <summary>
    /// 使用该Session调用一个api,
    /// 成功时返回调用结果,
    /// 无法被解析为实体时返回null,
    /// 表明失败时Result内部Data为null
    /// </summary>
    /// <param name="api">api实例</param>
    /// <param name="echo">echo, 任意在短时间内不会重复的值</param>
    /// <returns>api调用结果</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<CqApiCallResult?> CallApiAsync(CqApi api, string echo)
    {
        if (!UseApiEndPoint) throw new InvalidOperationException("This session doesn't use api endpoint.");
        if (!Receiving) throw new InvalidOperationException("This session hasn't started receiving.");

        string json = CqApiJsonSerializer.SerializeApi(api, echo);
        ArraySegment<byte> seg = new(Encoding.GetBytes(json));
        await webSocket.SendAsync(seg, WebSocketMessageType.Text, true, CancellationToken.None);

        CqApiCallResult? result = new();
        JsonDocument? docForDeserialize = null;

        AutoResetEvent e = new(false);
        void Callback(JsonDocument doc) => (_, docForDeserialize) = (e.Set(), doc);
        suspendedApiResponseWaitings.Add(echo, Callback);
        e.WaitOne();

        if (docForDeserialize is null) return null;
        result = CqApiJsonSerializer.DeserializeApiResult(docForDeserialize, api);
        if (result is null) return null;
        return result;
    }

    private static ArraySegment<byte> GetArraySegment(int size)
        => new(new byte[size]);
}