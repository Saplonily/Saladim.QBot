using System.Text;
using System.Text.Json;
using QBotDotnet.Core;
using QBotDotnet.GoCqHttp;
using QBotDotnet.GoCqHttp.Apis;
using QBotDotnet.GoCqHttp.Posts;
using QBotDotnet.SharedImplement;

namespace Test;

public class Program
{
    public static CqWebSocketClient client = null!;
    public static async Task Main()
    {
        client = new("127.0.0.1:5000");
    Start:
        try
        {
            await DoConnection();
        }
        catch (Exception ex)
        {
            if (ex.InnerException is not null)
            {
                StringBuilder sb = new();
                sb.Append("Chained exceptions: root >");
                foreach (var e in ExceptionHelper.GetChainedExceptions(ex))
                {
                    sb.Append($"--{e.Message}\n");
                }
                Console.WriteLine(sb.ToString());
            }
            else
            {
                Console.WriteLine($"Exception: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }
            Console.WriteLine("检测到客户端连接失败，将在3s后重新连接.");
            Thread.Sleep(3000);
            goto Start;
        }
        Console.WriteLine("---main method ended.---");
        Console.ReadLine();
    }
    private static void Client_OnPostAsync(CqPost? post)
    {
        if (post is null) return;
        if (post.PostType == CqPostType.MetaEvent) return;
        Console.WriteLine($"--收到上报类型: {post.PostType}");
        /**
        if (post is CqGroupMessagePost mpost)
        {
            Console.WriteLine($"{mpost.Sender.Nickname} 在群{mpost.GroupId}里说: {mpost.RawMessage},{mpost.MessageEntity}");
            SendGroupMessageEntityAction a = new()
            {
                GroupId = mpost.GroupId,
                AsCqCodeString = true,
                Message = mpost.MessageEntity
            };
            var result = (await client.CallApiAsync(a))!.Data!.Cast<SendGroupMessageActionResultData>();
            Console.WriteLine($"消息发送成功,id为{result.MessageId},api调用结果:{result.ResultIn.Status}");
        }
        if (post is CqPrivateMessagePost ppost)
        {
            Console.WriteLine($"消息临时来源: {ppost.TempSource},{ppost.Sender.Nickname}:{ppost.RawMessage}");
        }
        if (post is CqOtherMessagePost opost)
        {
            Console.WriteLine($"消息临时来源(other): {opost.TempSource},{opost.Sender.Nickname}:{opost.RawMessage}");
        }*/
        if (post is CqMessagePost mpost)
        {
            Console.WriteLine($"消息来咯({mpost.MessageId}): {mpost.Sender.Nickname}说:{mpost.RawMessage}");
            if (mpost.RawMessage.Contains("stop!"))
            {
                client.StopAsync();
            }
        }
        if (post is CqNoticePost np)
        {
            Console.WriteLine(post switch
            {
                CqGroupMemberIncreaseNoticePost np1 =>
                    $"有人进来群{np1.GroupId}了,人是{np1.UserId},操作者{np1.OperatorId},方式{np1.SubType}",
                CqGroupMemberDecreaseNoticePost np2 =>
                    $"有人退群{np2.GroupId}了,人是{np2.UserId},操作者{np2.OperatorId},方式{np2.SubType}",
                CqGroupAdminChangedNoticePost np3 =>
                    $"群{np3.GroupId}里的{np3.UserId}管理员变化了,类型是{np3.SubType}",
                CqGroupMemberBannedNoticePost np4 =>
                    $"""
                        群{np4.GroupId}里的{np4.UserId}被{np4.OperatorId}{(
                            np4.SubType == CqGroupMemberBannedNoticePost.NoticeSubType.Ban ?
                            $"禁言了{new TimeSpan(0, 0, (int)np4.Duration)}({np4.Duration}s)" :
                            "解禁了"
                        )}
                        """,
                CqGroupMessageRecalledNoticePost np5 =>
                    $"群{np5.GroupId}里{np5.UserId}的消息{np5.MessageId}被{np5.OperatorId}撤回了",
                CqFriendMessageRecalledNoticePost np6 =>
                    $"{np6.UserId}发给你的消息{np6.MessageId}被撤回啦",
                CqNotifyNoticePost np7 =>
                    $"{(np7 is CqPokeNotifyNoticePost pp ? $"{pp.SenderId}戳了戳{pp.TargetId},{pp.GroupId}" :
                    "unknow notify notice type")}",
                CqGroupMemberCardChangedNoticePost np8 =>
                    $"群{np8.GroupId}里的{np8.UserId}的名片从`{np8.CardOld}`变为了`{np8.CardNew}`",
                _ => $"未知的Notice接受,原始类型str是{np.NoticeType}"
            });
        }
        return;
    }

    private static async Task DoConnection()
    {
        await client.StartAsync();

        client.OnPost += Client_OnPostAsync;
        string s = string.Empty;
        while ((s = Console.ReadLine()!) != "exit")
        {
            SendGroupMessageAction api = new()
            {
                Message = s,
                GroupId = 860355679
            };
            _ = Task.Run(() => client.CallApiAsync(api));
        }
        await client.StopAsync();
    }
}