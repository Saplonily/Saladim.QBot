using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using QBotDotnet.Core;
using QBotDotnet.SharedImplement;

namespace QBotDotnet.GoCqHttp;

//TODO: 抽离WebSocket为Session
public sealed class CqWebSocketClient : IClient, IAsyncDisposable
{
    private readonly CqWebSocketSession sessionPost;
    private readonly CqWebSocketSession sessionApi;

    public const int BufferSize = 2 * 1024 * 1024;

    /// <summary>
    /// 在<see cref="StartAsync"/>执行成功后变为true,之后不再变化
    /// </summary>
    public bool StartedBefore { get; private set; }

    /// <summary>
    /// 该Client是否开启
    /// </summary>
    public bool Started { get; private set; }

    public event Action<CqPost?>? OnPost;

    public CqWebSocketClient(string gocqhttpWsAddress)
        => (sessionApi, sessionPost) =
           (new(gocqhttpWsAddress, StringConsts.CqApiEndpoint, false, true),
            new(gocqhttpWsAddress, StringConsts.CqPostEndpoint, true, false));

    private async Task ConnectWebSocketsSessionAsync()
    {
        CancellationToken token = CancellationToken.None;

        await sessionPost.ConnectAsync(token);
        await sessionApi.ConnectAsync(token);
    }

    public async Task<CqApiCallResult?> CallApiAsync(CqApi api)
    {
        return await sessionApi.SendApi(api, Guid.NewGuid().ToString());
    }

    #region Start & Stops 实现

    public Task StartAsync()
    {
        if (Started)
            throw new ClientException(this, ClientException.ExceptionType.AlreadyStarted);
        return InternalStartAsync();
    }

    public Task StopAsync()
    {
        if (!StartedBefore)
            throw new ClientException(this, ClientException.ExceptionType.NotStartedBefore);
        if (!Started)
            throw new ClientException(this, ClientException.ExceptionType.AlreadyStopped);
        return InternalStopAsync();
    }

    internal async Task InternalStartAsync()
    {
        static void CheckAndRenewWs(CqWebSocketSession s)
        {
            if (s.State is WebSocketState.Closed or WebSocketState.Aborted)
                s.RenewWebSocket();
        }

        ObjectHelper.BulkRun(CheckAndRenewWs, sessionApi, sessionPost);
        try
        {
            await ConnectWebSocketsSessionAsync();

            _ = sessionPost.StartReceiving();
            sessionPost.OnReceived += (in JsonDocument srcDoc) =>
            {
                CqPost? post = JsonSerializer.Deserialize<CqPost>(srcDoc, CqJsonOptions.Instance);
                Task.Run(() => OnPost?.Invoke(post));
            };
            _ = sessionApi.StartReceiving();

            StartedBefore = true;
            Started = true;
        }
        catch (WebSocketException wex)
        {
            Started = false;
            throw new ClientException(this,
                ClientException.ExceptionType.WebSocketError,
                innerException: wex,
                message: $"Internal WebSocket error({wex.WebSocketErrorCode}), " +
                $"please check the inner exceptions."
                );
        }

        return;
    }

    internal async Task InternalStopAsync()
    {
        if (!Started)
        {
            var token = CancellationToken.None;
            string des = "The client called the stop method.";
            await ObjectHelper.BulkRunAsync(
                s => s.CloseAsync(WebSocketCloseStatus.NormalClosure, des, token),
                sessionApi, sessionPost);
        }
        return;
    }

    public async ValueTask DisposeAsync()
    {
        await InternalStopAsync();
        return;
    }

    #endregion
}