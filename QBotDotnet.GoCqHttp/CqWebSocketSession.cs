using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using QBotDotnet.SharedImplement;

namespace QBotDotnet.GoCqHttp;

public sealed class CqWebSocketSession : IDisposable
{
    public const int BufferSize = 1 * Size.MiB;
    public static readonly Encoding Encoding = Encoding.UTF8;
    public readonly Uri Uri;
    private ClientWebSocket webSocket = new();
    private CancellationTokenSource receiveTokenSource = new();
    private readonly Dictionary<string, Action<JsonDocument>> suspendApiResponseWaitings = new();

    public bool UseEventEndPoint { get; private set; }
    public bool UseApiEndPoint { get; private set; }
    public Task? ReceivingTask { get; private set; }
    public bool Receiving { get; private set; }
    public WebSocketState State { get => webSocket.State; }

    public delegate void OnReceivedHandler(in JsonDocument parseDocment);
    public event OnReceivedHandler? OnReceived;

    public CqWebSocketSession(string address, string? endpoint = null)
    {
        Uri = new($"ws://{address}/{endpoint}");
    }

    public CqWebSocketSession(string address, string? endpoint = null,
        bool useEventEndPoint = false, bool useApiEndPoint = false)
        : this(address, endpoint) =>
        (UseEventEndPoint, UseApiEndPoint) =
        (useEventEndPoint, useApiEndPoint);

    public void Dispose()
        => webSocket.Dispose();

    public Task ConnectAsync(CancellationToken token)
        => webSocket.ConnectAsync(Uri, token);

    public Task CloseAsync(WebSocketCloseStatus status, string? description, CancellationToken token)
        => webSocket.CloseAsync(status, description, token);

    public void Abort()
        => webSocket.Abort();

    public void RenewWebSocket()
    {
        if (webSocket.State is WebSocketState.Aborted or WebSocketState.Closed)
        {
            webSocket.Abort();
            webSocket.Dispose();
            webSocket = new();
        }
    }

    public Task EmitOnReceived(JsonDocument docToEmit) => Task.Run(() =>
    {
        if (this.UseEventEndPoint)
        {
            OnReceived?.Invoke(docToEmit);
        }
        if (this.UseApiEndPoint && docToEmit.RootElement.TryGetProperty(StringConsts.ApiEchoProperty, out var echoProp))
        {
            try
            {
                if (suspendApiResponseWaitings.TryGetValue(
                    echoProp.GetString() ?? string.Empty, out var action)
                )
                    action.Invoke(docToEmit);

            }
            catch (InvalidOperationException) { } //忽略取不到的异常
        }
    });

    public Task StartReceiving() => ReceivingTask = Task.Run(() =>
    {
        var segment = GetArraySegment(BufferSize);
        try
        {
            Receiving = true;
            while (true)
            {
                var result = webSocket.ReceiveAsync(segment, receiveTokenSource.Token).Result;
                //懒,出bug后再检测result的一些状态
                string str = Encoding.GetString(segment.Array!, segment.Offset, result.Count);
                this.EmitOnReceived(JsonDocument.Parse(str));
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
    });

    public void StopReceiving()
    {
        receiveTokenSource.Cancel();
    }

    public async Task<CqApiCallResult?> SendApi(CqApi api, string echo)
    {
        if (!UseApiEndPoint) throw new InvalidOperationException("This session don't use api endpoint.");
        if (!Receiving) throw new InvalidOperationException("This session haven't started receiving.");

        ArraySegment<byte> seg = new(Encoding.GetBytes(CqApiJsonSerializer.SerializeApi(api, echo)));
        await webSocket.SendAsync(seg, WebSocketMessageType.Text, true, CancellationToken.None);
        AutoResetEvent e = new(false);
        CqApiCallResult? result = new();
        JsonDocument? docForDeserialize = null;

        void Callback(JsonDocument doc) => (_, docForDeserialize) = (e.Set(), doc);
        suspendApiResponseWaitings.Add(echo, Callback);
        e.WaitOne();
        if (docForDeserialize is null) return null;

        result = CqApiJsonSerializer.DeserializeApiResult(docForDeserialize, api);

        if (result is null) return null;

        if (result.Wording is not null) throw new CqApiCallFailedException(result);

        return result;
    }

    private static ArraySegment<byte> GetArraySegment(int size)
        => new(new byte[size]);
}