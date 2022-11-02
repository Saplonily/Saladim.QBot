using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace QBotDotnet.GoCqHttp;

[DebuggerDisplay("Api-{ApiName}")]
public abstract class CQApi
{
    public virtual string ApiName { get; } = "null";
    public virtual bool UseCache { get; internal protected set; } = false;

    /// <summary>
    /// 同步发送一个api请求
    /// </summary>
    /// <param name="ws">api websocket</param>
    /// <param name="echo">echo</param>
    internal void Send(ClientWebSocket ws, string echo)
    {
        string j = GenerateJson(this, echo);
        ws.SendAsync(
            new ArraySegment<byte>(
            Encoding.UTF8.GetBytes(j)
            ),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
            );
    }
}