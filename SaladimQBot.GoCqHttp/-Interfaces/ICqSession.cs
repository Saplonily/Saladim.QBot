namespace SaladimQBot.GoCqHttp;

public interface ICqSession
{
    event OnCqSessionReceivedHandler? OnReceived;

    Task<CqApiCallResult?> CallApiAsync(CqApi api, string echo);
}