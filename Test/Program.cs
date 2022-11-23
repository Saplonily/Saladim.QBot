using System.Reflection;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace Test;

public static class Program
{
    private static CqWebSocketClient client = null!;
    private static Logger logger = null!;
    private static StreamWriter writer = null!;
    public const string SecProgram = "Program";
    public static async Task Main()
    {
        string rawLogFileName = $"D:\\Projects\\Saladim.QBot\\Logs\\{DateTime.Now.ToShortDateString()}";
        int index = 0;
        while (File.Exists($"{rawLogFileName}-{index}.log"))
        {
            index++;
        }
        writer = new($"{rawLogFileName}-{index}.log");
        logger = new LoggerBuilder()
            .WithLevelLimit(LogLevel.Trace)
            .WithLogToConsole()
            .WithAction(s => writer.WriteLine(s))
            .Build();
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            try
            {
                writer.Dispose();
            }
            catch { }
        }

        client = new("127.0.0.1:5000", LogLevel.Trace);
    Start:
        try
        {
            await client.StartAsync();

            /*client.OnPost += Client_OnPostAsync;*/
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnLog += s => logger.LogInfo("External", "GoCqHttpClient", s);
            ConsoleLoop();
            await client.StopAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(SecProgram, "MainMethod", ex);
            logger.LogError(SecProgram, "检测到客户端连接失败，将在3s后重新连接.");
            Thread.Sleep(3000);
            goto Start;
        }
        logger.LogInfo(SecProgram, "---main method ended.---");
        writer.Dispose();
        Console.ReadLine();
    }

    private static void Client_OnMessageReceived(Message msg)
    {
        Task.Run(OnMessageReceived);
        void OnMessageReceived()
        {
            if (msg is GroupMessage message)
            {
                if (message.MessageEntity.Value.RawString.Contains("/random"))
                {
                    Random r = new();
                    int num = r.Next(0, 100);
                    message.Group.Value.SendMessageAsync($"{message.Sender.Value.CqAt} 随机数为{num}哦~");
                }
            }
            else
            {
                if (msg.MessageEntity.Value.RawString.Contains("/random"))
                {
                    Random r = new();
                    int num = r.Next(0, 100);
                    msg.Sender.Value.SendMessageAsync($"随机数为{num}哦~");
                }
            }
        }
    }

    private static void Client_OnPostAsync(CqPost? post)
    {
        if (post is null) return;
        if (post.PostType == CqPostType.MetaEvent) return;
        logger.LogTrace(SecProgram, $"--收到上报类型: {post.PostType}");
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
            logger.LogDebug(SecProgram, $"消息来咯({mpost.MessageId}): {mpost.Sender.Nickname}说:{mpost.RawMessage}");
            if (mpost.RawMessage.Contains("stop!"))
            {
                client.StopAsync();
            }
            if (mpost is CqGroupMessagePost gpost)
            {
                var gsender = mpost.Sender.AsCast<CqGroupMessageSender>()!;
                logger.LogDebug(SecProgram, $"这是一条群消息, 群号是{gpost.GroupId}");
                string s = "/echo ";
                if (mpost.RawMessage.StartsWith(s))
                {
                    SendGroupMessageAction api = new()
                    {
                        GroupId = gpost.GroupId,
                        Message = gpost.RawMessage.Substring(s.Length)
                    };
                    client.CallApiAsync(api).ContinueWith(task =>
                    {
                        logger.LogDebug(SecProgram, $"API调用了,结果是{task.Result?.Data}");
                    });
                }
                if (mpost.RawMessage.Contains("repeatTHIS"))
                {
                    SendGroupMessageAction api = new()
                    {
                        GroupId = gpost.GroupId,
                        Message = mpost.RawMessage
                    };
                    client.CallApiAsync(api).ContinueWith(task =>
                    {
                        logger.LogDebug(SecProgram, $"API调用了,结果是{task.Result?.Data}");
                    });
                }
            }
        }
        if (post is CqNoticePost np)
        {
            logger.LogInfo(SecProgram, post switch
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

    private static void ConsoleLoop()
    {
        string s;
        while ((s = Console.ReadLine()!) != "exit")
        {
            if (s.Contains("start!"))
            {
                try
                {
                    _ = client.StartAsync();
                }
                catch (ClientException) { }
                continue;
            }
        }
    }
}