using System.Text.Json;

namespace SaladimQBot.GoCqHttp;

public interface ICqSession : IDisposable
{
    bool Started { get; }

    Task StartAsync();

    Task<CqApiCallResult?> CallApiAsync(CqApi api);

    void EmitOnReceived(JsonDocument docToEmit);

    event OnCqSessionReceivedHandler OnReceived;
}