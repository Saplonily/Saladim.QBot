using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using CodingSeb.ExpressionEvaluator;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace Test;

public static class Program
{
    private static CqClient client = null!;
    private static Logger logger = null!;
    private static StreamWriter writer = null!;
    public const string SecProgram = "Program";
    public static async Task Main()
    {
        //大部分情况下你会因为没有这个目录导致程序直接寄
        #region 初始化/配置日志
        //搞日志:
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
        #endregion

        logger.LogInfo(SecProgram, "Starting...");
        client = new CqHttpClient("http://127.0.0.1:5700", "http://127.0.0.1:5566", LogLevel.Trace);
        client.OnPost += Client_OnPostAsync;
        client.OnMessageReceived += Client_OnMessageReceived;
        client.OnLog += s => logger.LogInfo("External", "GoCqHttpClient", s);

        bool connected = false;
        while (connected == false)
        {
            try
            {
                await client.StartAsync();
                logger.LogInfo(SecProgram, "cq客户端连接成功.");
                connected = true;
            }
            catch (Exception ex)
            {
                connected = false;
                logger.LogError(SecProgram, "MainMethod", ex);
                logger.LogError(SecProgram, "检测到客户端连接失败，将在3s后重新连接.");
                Thread.Sleep(3000);
            }
        }

        await ConsoleLoop();

        try
        {
            await client.StopAsync();
        }
        catch (ClientException e)
        {
            logger.LogWarn("Program", "Client", e);
        }
        logger.LogInfo(SecProgram, "---main method ended.---");
        writer.Dispose();
    }

    private static void Client_OnMessageReceived(Message message)
    {
        Task.Run(OnMessageReceived);
        void OnMessageReceived()
        {
            string rawString = message.MessageEntity.RawString;

            if (rawString.Contains("/random"))
            {
                Random r = new();
                int num = r.Next(0, 100);
                message.MessageWindow.SendMessageAsync($"{message.Sender.CqAt} {message.Sender.Nickname.Value},你的随机数为{num}哦~");
            }
            if (rawString.Contains("/来点图"))
            {
                int count = rawString.Count("/来点图");
                for (int i = 0; i < count; i++)
                {
                    HttpClient client = new();
                    var result = client.GetAsync("https://img.xjh.me/random_img.php?return=json").Result;
                    StreamReader reader = new(result.Content.ReadAsStream());
                    string s = reader.ReadToEnd();
                    JsonDocument doc = JsonDocument.Parse(s);
                    string imageUrl = "http:" + doc.RootElement.GetProperty("img").GetString()!;
                    CqMessageEntity cqEntity = new()
                    {
                        new CqMessageImageSendNode(imageUrl)
                    };
                    message.MessageWindow.SendMessageAsync(new MessageEntity(cqEntity));
                }
            }
            string whatCalculate = "/算 ";
            if (rawString.StartsWith(whatCalculate))
            {
                try
                {
                    string thing = rawString[whatCalculate.Length..];
                    ExpressionEvaluator e = new();
                    string rst = e.Evaluate(thing)?.ToString() ?? "";
                    message.MessageWindow.SendMessageAsync($"计算结果是: {rst}");
                }
                catch (Exception e)
                {
                    logger.LogWarn("Program", "Calculate", e);
                }
            }

            if (rawString.Contains("撤回这条消息"))
            {
                var replyNode =
                    (from node in message.MessageEntity.CqEntity
                     where node is CqMessageReplyIdNode
                     let replyIdNode = node as CqMessageReplyIdNode
                     select replyIdNode).First();
                if (replyNode is not null)
                {
                    _ = client.RecallMessageAsync(replyNode.MessageId);
                }
            }
        }
    }

    //操控原始上报
    private static void Client_OnPostAsync(CqPost? post)
    {
        if (post is null) return;
        if (post.PostType == CqPostType.MetaEvent) return;
        logger.LogTrace(SecProgram, $"--收到上报类型: {post.PostType}");

        if (post is CqPrivateMessagePost ppost)
        {
            logger.LogDebug("Program", "RawPost", $"消息临时来源: {ppost.TempSource},{ppost.Sender.Nickname}:{ppost.RawMessage}");
        }

        if (post is CqOtherMessagePost opost)
        {
            logger.LogDebug(
                "Program", "RawPost", $"消息临时来源(other): {opost.TempSource}," +
                $"{opost.Sender.Nickname}:{opost.RawMessage}"
                );
        }

        if (post is CqMessagePost mpost)
        {
            logger.LogDebug(
                "Program", "RawPost", $"消息来咯({mpost.MessageId}): " +
                $"{mpost.Sender.Nickname}说:{mpost.RawMessage}"
                );
            if (mpost is CqGroupMessagePost gpost)
            {
                var gsender = mpost.Sender.AsCast<CqGroupMessageSender>()!;
                logger.LogDebug("Program", "RawPost", $"这是一条群消息, 群号是{gpost.GroupId}");
                string s = "/echo ";
                if (mpost.RawMessage.StartsWith(s))
                {
                    _ = client.SendGroupMessageAsync(gpost.GroupId, gpost.RawMessage.Substring(s.Length));
                }
            }
        }
        if (post is CqNoticePost np)
        {
            logger.LogDebug("Program", "RawPost", post switch
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

    private async static Task ConsoleLoop()
    {
        string s;
        while ((s = Console.ReadLine()!) != "exit")
        {
            try
            {
                if (s is "stop")
                {
                    await client.StopAsync();
                    logger.LogInfo(SecProgram, "stop动作完成.");
                }
                if (s is "start")
                {
                    await client.StartAsync();
                    logger.LogInfo(SecProgram, "start动作完成.");
                }
            }
            catch (Exception e)
            {
                logger.LogWarn(SecProgram, "Exception", e);
            }
        }
    }
}