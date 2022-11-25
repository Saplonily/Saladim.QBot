using System.Net.WebSockets;
using System.Text.Json;
using Saladim.SalLogger;
using SaladimQBot.Shared;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using System.Text.RegularExpressions;

namespace SaladimQBot.GoCqHttp;

public sealed class CqWebSocketClient : CqClient
{
    private readonly CqWebSocketSession apiSession;
    private readonly CqWebSocketSession postSession;
    private readonly TimeSpan expireTime = new(0, 3, 14);

    public override ICqSession ApiSession => apiSession;

    public override ICqSession PostSession => postSession;

    public override TimeSpan ExpireTimeSpan => expireTime;

    public CqWebSocketClient(string gocqHttpAddress, LogLevel logLevelLimit = LogLevel.Info) : base(logLevelLimit)
    {
        apiSession = new(gocqHttpAddress, "api", useApiEndPoint: true);
        postSession = new(gocqHttpAddress, "event", useEventEndPoint: true);

    }
}