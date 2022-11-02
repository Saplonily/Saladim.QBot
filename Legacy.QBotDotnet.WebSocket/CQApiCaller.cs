using System.Net.WebSockets;
using System.Text.Json;

namespace QBotDotnet.GoCqHttp;

public class CQApiCaller
{
    protected Client client;
    protected ClientWebSocket wsApi;
    public CQApiCaller(ClientWebSocket wsApi, Client client)
    {
        this.wsApi = wsApi;
        this.client = client;
    }
    /// <summary>
    /// 发送一个请求
    /// </summary>
    /// <param name="api">具体的api action</param>
    /// <param name="echo">echo值</param>
    public async Task<ApiActionResult> SendAsync(CQApi api, string echo)
    {
        await Task.Run(() => api.Send(wsApi, echo));

        var eEvent = new AutoResetEvent(false);
        string msg = "";
        void onResponse(in string responseEcho, in string responseMsg)
        {
            if (responseEcho == echo)
            {
                eEvent.Set();
                msg = responseMsg;
            }
        }
        client.OnResponse += onResponse;
        eEvent.WaitOne();
        client.OnResponse -= onResponse;
        var je = JsonDocument.Parse(msg).RootElement;
        return ApiActionResult.GetFrom(je);
    }

    public async Task<ApiActionResult> SendAsync(CQApi api)
    {
        return await SendAsync(api, Guid.NewGuid().ToString());
    }
}