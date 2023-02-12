using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("httpSession, Started={Started}, url={goCqHttpAddressBaseUrl}")]
public class CqHttpRequestorSession : ICqSession, IDisposable
{
    protected HttpClient httpClient = null!;
    protected string goCqHttpAddressBaseUrl;

#pragma warning disable 
    public event OnCqSessionReceivedHandler? OnReceived;

    public event Action<Exception>? OnErrorOccurred;
#pragma warning restore

    public bool Started { get; protected set; }
    public string? Authorization { get; }

    /// <summary>
    /// 使用http地址创建一个
    /// </summary>
    /// <param name="goCqHttpAddressBaseUrl"></param>
    public CqHttpRequestorSession(string goCqHttpAddressBaseUrl, string? authorization = null)
    {
        this.goCqHttpAddressBaseUrl = goCqHttpAddressBaseUrl;
        this.Authorization = authorization;
    }

    public async Task<(CqApiCallResult? result, int statusCode)> CallApiAsync(CqApi api)
    {
        var url = $"{goCqHttpAddressBaseUrl}/{api.ApiName}";
        JsonObject apiParamsNode = CqApiJsonSerializer.SerializeApiParamsToNode(api);
        string jsonString = apiParamsNode.ToJsonString();
        StringContent content = new(jsonString);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        content.Headers.Add("Authorization", Authorization);
        HttpResponseMessage? response;
        try
        {
            response = await httpClient.PostAsync(url, content).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            return (null, 22);
        }
        response.EnsureSuccessStatusCode();
        var resultStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        JsonDocument? doc;
        try
        {
            doc = await JsonDocument.ParseAsync(resultStream).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return (null, 20);
        }
        var deserializedResult = CqApiJsonSerializer.DeserializeApiResult(doc, api);
        if (deserializedResult is null) return (null, 23);
        if (deserializedResult.Data is null) return (deserializedResult, 21);
        return (deserializedResult, 10);
    }

    ~CqHttpRequestorSession()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            httpClient.Dispose();
        }
    }

    public Task StartAsync()
    {
        Started = true;
        try
        {
            httpClient = new()
            {
                Timeout = new TimeSpan(0, 0, 20)
            };
        }
        catch (Exception)
        {
            Started = false;
            throw;
        }
        return Task.CompletedTask;
    }
}
