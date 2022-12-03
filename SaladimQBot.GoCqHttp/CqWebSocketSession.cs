using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public delegate void OnCqSessionReceivedHandler(in JsonDocument parsedDocument);

public sealed partial class CqWebSocketSession : ICqSession, IDisposable
{
    public const int BufferSize = 1 * Size.MiB;
    public static readonly Encoding Encoding = Encoding.UTF8;
    public readonly Uri Uri;

    private readonly Dictionary<string, Action<JsonDocument>> suspendedApiResponseWaitings = new();
    private ClientWebSocket webSocket = new();
    private CancellationTokenSource receiveTokenSource = new();
    private int seq = 0;

    /// <summary>
    /// 是否使用事件端口
    /// </summary>
    public bool UseEventEndPoint { get; private set; } = false;

    /// <summary>
    /// 是否使用api端口
    /// </summary>
    public bool UseApiEndPoint { get; private set; } = false;

    public Task? ReceivingTask { get; private set; }

    public WebSocketState State { get => webSocket.State; }

    public bool Started { get; private set; }


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
        Uri = new($"{address}/{endpoint}");
    }

    public CqWebSocketSession(string address, string? endpoint = null,
        bool useEventEndPoint = false, bool useApiEndPoint = false)
        : this(address, endpoint) =>
        (UseEventEndPoint, UseApiEndPoint) =
        (useEventEndPoint, useApiEndPoint);

    private async Task ConnectAsync(CancellationToken token)
    {
        try
        {
            MakeWebSocketAvailable();
            await webSocket.ConnectAsync(Uri, token);
            Started = true;
            ReceivingTask = Task.Run(ReceivingLoop);
        }
        catch (Exception)
        {
            Started = false;
            throw;
        }
        void MakeWebSocketAvailable()
        {
            if (webSocket.State is WebSocketState.Aborted or WebSocketState.Closed)
            {
                webSocket.Abort();
                webSocket.Dispose();
                webSocket = new();
                this.ReceivingTask = null;
            }
        }
    }

    public void Dispose()
    {
        receiveTokenSource.Cancel();
        receiveTokenSource = new();
        ReceivingTask = null;
        webSocket.Dispose();
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

    private void ReceivingLoop()
    {
        var segment = GetArraySegment(BufferSize);
        try
        {
            Started = true;
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
                throw;
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        finally
        {
            ReceivingTask = null;
            Started = false;
            webSocket.Abort();
            webSocket.Dispose();
        }
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
        if (!Started) throw new InvalidOperationException("This session hasn't started receiving.");

        //根节点
        JsonObject rootObj = new(new Dictionary<string, JsonNode?>()
        {
            [StringConsts.ActionProperty] = JsonValue.Create(api.ApiName),
            [StringConsts.ApiEchoProperty] = JsonValue.Create(echo),
        });
        //获得参数对象节点
        JsonObject paramObject = CqApiJsonSerializer.SerializeApiParamsToNode(api);
        rootObj.Add(StringConsts.ParamsProperty, paramObject);
        //转为json字符串
        string json = rootObj.ToJsonString();

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

    Task ICqSession.StartAsync()
        => ConnectAsync(CancellationToken.None);

    Task<CqApiCallResult?> ICqSession.CallApiAsync(CqApi api)
        => CallApiAsync(api, seq++.ToString());
}