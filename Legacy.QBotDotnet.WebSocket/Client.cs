using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using QBotDotnet.Core;
using QBotDotnet.GoCqHttp;
using QBotDotnet.GoCqHttp.Internal;
using QBotDotnet.Public;

namespace QBotDotnet;
public class Client : IClient
{
    private ClientWebSocket wsPost = null!;
    private ClientWebSocket wsApi = null!;
    private CQApiCaller apiCaller = null!;
    private readonly int port;
    private Task? acceptEventTask;
    private Task? apiSendTask;

    internal delegate void OnResponseHandler(in string echo, in string responseString);
    internal event OnResponseHandler? OnResponse;
    internal CQApiCaller ApiCaller { get => apiCaller; }

    public delegate void OnMessageArrivedHandler(Message message);

    public event OnMessageArrivedHandler? OnMessageArrived;
    public bool Connected { get; protected set; }


    /// <summary>
    /// 初始化一个Client
    /// </summary>
    /// <param name="port">websocket端口号,默认5000</param>
    public Client(int port = 5000)
    {
        Init();
        this.port = port;
    }

    internal void Init()
    {
        wsPost = new();
        wsApi = new();
        apiCaller = new(wsApi, this);
    }

    /// <summary>
    /// 启动该客户端
    /// </summary>
    /// <returns>是否成功启动</returns>
    public async Task<bool> StartAsync()
    {
        if (Connected) throw new Exceptions.ClientAlreadyStartedException();
        try
        {
            await wsPost.ConnectAsync(new Uri($"ws://127.0.0.1:{port}"), CancellationToken.None);
            await wsApi.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/api"), CancellationToken.None);
            Connected = true;
        }
        catch (Exception ex)
        {
            Connected = false;
            Logger.LogInfo($"连接失败: {ex.Message}\n{ex.StackTrace}");
            Init();
            return false;
        }
        Logger.LogInfo("连接成功");
        //接受上报的任务
        acceptEventTask = Task.Run(async () =>
        {
            try
            {
                ArraySegment<byte> seg = new(new byte[1024 * 1024 * 2]);
                while (true)
                {
                    var result = await wsPost.ReceiveAsync(seg, CancellationToken.None);
                    var segArray = seg.Array;
                    if (segArray is null) continue;
                    string msg = Encoding.UTF8.GetString(segArray, 0, result.Count);
                    OnPostMessage(in msg);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex.Message);
                Debug.Assert(ex.StackTrace != null);
                Logger.LogException(ex.StackTrace);
            }
        });
        //接受api请求返回的任务
        apiSendTask = Task.Run(async () =>
        {
            try
            {
                ArraySegment<byte> seg = new(new byte[1024 * 1024 * 1]);
                while (true)
                {
                    var result = await wsApi.ReceiveAsync(seg, CancellationToken.None);
                    var segArray = seg.Array;
                    if (segArray is null) continue;
                    string msg = Encoding.UTF8.GetString(segArray, 0, result.Count);
                    OnApiEcho(in msg);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex.Message);
                Debug.Assert(ex.StackTrace != null);
                Logger.LogException(ex.StackTrace);
            }
        });
        return true;
    }

    public async void StopAsync()
    {
        await wsPost.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        await wsApi.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }

    private void OnPostMessage(in string msg)
    {
        var msgProcessed = msg;
        //这可能是个cq-http的bug,匿名消息收到时会出现两个sub_type的key
        //一个值为normal,一个值为anonymous
        //这里检测是否这个非法json字符串同时存在这两个键
        //是的话就移除sub_type为normal的键值对
        const string subErr1 = "\"sub_type\": \"normal\"";
        const string subErr2 = "\"sub_type\": \"anonymous\"";
        if (msg.Contains(subErr1) && msg.Contains(subErr2))
            msgProcessed = msg.Replace(subErr1, string.Empty);
        var jr = JsonDocument.Parse(msgProcessed).RootElement;
        CQPost post = CQPost.GetFrom(jr, true);
        ProcessPost(post);
    }

    private async void ProcessPost(CQPost post)
    {
        if (post.PostType == CQPost.CQPostType.MetaEvent) return;
        if (post is CQMessagePost msgPost)
        {
            OnMessageArrived?.Invoke(new Message(msgPost, this));
        }
        switch (post)
        {
            case CQGroupMessagePost mp:
                if (mp.RawMessage.Contains("!echo "))
                {
                    MessageBuilder builder = new();
                    builder.AddText(mp.RawMessage[6..]);
                    var result = await apiCaller.SendAsync(
                        new CQSendPrivateMessageAction(mp.Sender.UserId, builder.Build())
                        );
                    Logger.LogInfo($"result is {result.Status}");
                    Logger.LogInfo($"群{mp.GroupId}来消息了 {mp.UserId}说: {mp.RawMessage}");
                }
                if (mp.RawMessage.Contains("check_me"))
                {
                    var result = await apiCaller.SendAsync(new CQGetGroupMemberInfoAction(mp.GroupId, mp.UserId));
                    string rst = JsonSerializer.Serialize(result.Data);
                    var msg = new MessageBuilder().AddText(rst).Build();
                    await apiCaller.SendAsync(new CQSendGroupMessageAction(mp.GroupId, msg));
                }
                break;
            case CQPrivateMessagePost mp:
                Logger.LogInfo($"{mp.Sender.UserId}私聊给你: {mp.RawMessage}");

                break;

            case CQGroupUploadNoticePost np:
                Logger.LogInfo($"有人上传文件了~ Name:{np.FileName} Size:{np.FileSize}B Url:{np.FileUrl}");
                break;

            case CQAdminChangeNoticePost np:
                Logger.LogInfo($"有人的管理员变更力,{np.GroupId}群里的{np.UserId}被{(np.IsSet ?? false ? "设为管理员" : "取消管理员")}");
                break;

            case CQGroupIncreaseNoticePost np:
                Logger.LogInfo($"呀~有新人进来了,你好~{np.UserId},加入方式为{np.Way}");
                break;

            case CQGroupDecreaseNoticePost np:
                Logger.LogInfo($"呜呜呜,有人走了,{np.UserId}再见,离开方式为{np.Way},操作者是{np.OperatorId}");
                break;

            case CQGroupBanNoticePost np:
                Logger.LogInfo(
                    $"有人被{(np.IsLiftBan ? "解禁了" : "禁言")}力,幸运儿是{np.UserId},管理员是{np.OperatorId}" +
                    $"{(np.IsLiftBan ? "" : $",时长有足足{np.Duration}秒哦~")}"
                    );
                break;

            case CQFriendAddNoticePost np:
                Logger.LogInfo($"{np.UserId}添加你为好友啦~");
                break;

            case CQGroupRecallNoticePost np:
                Logger.LogInfo($"{np.GroupId}群里的{np.OperatorId}撤回了{np.UserId}的一条消息{np.MessageId}...");
                break;

            case CQFriendRecallNoticePost np:
                Logger.LogInfo($"{np.UserId}撤回了一条给你发的消息~消息id是{np.MessageId}");
                break;

            case CQGroupCardChangeNoticePost np:
                Logger.LogInfo($"""
                        {np.UserId}改名字啦~,从"{np.CardOld}"变为"{np.CardNew}"
                        """);
                break;

            case CQFriendFileNoticePost np:
                Logger.LogInfo($"""
                        芜湖,{np.UserId}给你发文件了,叫"{np.FileName}",大小足足有{np.FileSize}B那么大,
                        你可以在这里下载:{np.FileUrl}
                        """);
                break;

        }
    }
    private void OnApiEcho(in string msg)
    {
        Logger.LogInfo($"[ApiEcho] {msg}");
        var rootJE = JsonDocument.Parse(msg).RootElement;
        string echo = rootJE.GetProperty("echo").GetString() ?? "";

        OnResponse?.Invoke(in echo, in msg);
    }
}