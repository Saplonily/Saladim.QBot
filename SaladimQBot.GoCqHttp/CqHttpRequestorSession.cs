using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("httpSession, Started={Started}, url={goCqHttpAddressBaseUrl}")]
public class CqHttpRequestorSession : ICqSession, IDisposable
{
    protected HttpClient httpClient = null!;
    protected string goCqHttpAddressBaseUrl;

#pragma warning disable CS0067
    public event OnCqSessionReceivedHandler? OnReceived;
#pragma warning restore CS0067


    public bool Started { get; protected set; }

    /// <summary>
    /// 使用http地址创建一个
    /// </summary>
    /// <param name="goCqHttpAddressBaseUrl"></param>
    public CqHttpRequestorSession(string goCqHttpAddressBaseUrl)
    {
        this.goCqHttpAddressBaseUrl = goCqHttpAddressBaseUrl;
    }

    public async Task<CqApiCallResult?> CallApiAsync(CqApi api)
    {
        var url = $"{goCqHttpAddressBaseUrl}/{api.ApiName}";
        JsonObject apiParamsNode = CqApiJsonSerializer.SerializeApiParamsToNode(api);
        string jsonString = apiParamsNode.ToJsonString();
        StringContent content = new(jsonString);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return CqApiJsonSerializer.DeserializeApiResult(doc, api);
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }

    public Task StartAsync()
    {
        Started = true;
        try
        {
            httpClient = new();
            httpClient.Timeout = new TimeSpan(0, 0, 20);
        }
        catch (Exception)
        {
            Started = false;
            throw;
        }
        return Task.CompletedTask;
    }
}
