﻿using System.Text.Json;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace Test;

public static class Program
{
    private static CqClient client = null!;
    private static Logger logger = null!;
    private static StreamWriter writer = null!;
    public const string SecProgram = "Program";

    private async static Task ConsoleLoop()
    {
        string s;
        while ((s = Console.ReadLine()!) != "exit")
        {
            try
            {
                if (s is "stop")
                {
                    await client.StopAsync().ConfigureAwait(false);
                    logger.LogInfo(SecProgram, "stop动作完成.");
                }
                if (s is "start")
                {
                    await client.StartAsync().ConfigureAwait(false);
                    logger.LogInfo(SecProgram, "start动作完成.");
                }
            }
            catch (Exception e)
            {
                logger.LogWarn(SecProgram, "Exception", e);
            }
        }
    }

    public static async Task<int> Main()
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
        client = new CqWebSocketClient("ws://127.0.0.1:5000", LogLevel.Trace);
        SubscribeEvents();

        bool connected = false;
        while (connected == false)
        {
            try
            {
                await client.StartAsync().ConfigureAwait(false);
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

        await ConsoleLoop().ConfigureAwait(false);

        try
        {
            await client.StopAsync().ConfigureAwait(false);
        }
        catch (ClientException e)
        {
            logger.LogWarn("Program", "Client", e);
        }
        logger.LogInfo(SecProgram, "Main method ended.");
        writer.Dispose();
        return 0;

        static void SubscribeEvents()
        {
            client.OnLog += s => logger.LogInfo("External", "GoCqHttpClient", s);
            client.OnGroupMemberIncreased += Client_OnGroupMemberIncreased;
            client.OnGroupMemberChanged += Client_OnGroupMemberChanged;
            client.OnGroupMemberDecreased += Client_OnGroupMemberDecreased;
            client.OnGroupMessageReceived += Client_OnGroupMessageReceived;
            client.OnGroupAllUserBanned += Client_OnGroupAllUserBanned;
            client.OnFriendMessageReceived += Client_OnFriendMessageReceived;
            client.OnFriendAddRequested += Client_OnFriendAddRequested;
            client.OnFriendAddRequested += Client_OnFriendAddRequested;
        }
    }

    private static async void Client_OnFriendAddRequested(FriendAddRequest request)
    {
        logger.LogInfo("Program", $"有人想加你好友, 叫做{request.User.Nickname.Value}, 验证消息:\"{request.Comment}\"");
        await request.ApproveAsync().ConfigureAwait(false);
    }

    private static void Client_OnFriendMessageReceived(FriendMessage message, FriendUser friendUser)
    {
        logger.LogInfo("Program", $"好友{friendUser.Nickname.Value}给你发消息了: {message.MessageEntity.RawString}");
    }

    private static void Client_OnGroupAllUserBanned(JoinedGroup group, GroupUser operatorUser)
    {
        logger.LogInfo("Program", $"{group.Name.Value}里的屑{operatorUser.FullName}打开了全员禁言.");
    }

    private static async void Client_OnGroupMessageReceived(GroupMessage message, JoinedGroup group)
    {
        var entity = message.MessageEntity;
        if (group.GroupId != 860355679 || message.Sender.UserId == 2259113381) return;
        if (entity.Mentioned(client.Self))
        {
            await message.ReplyAsync($"qwq, 你@我了, 然后你这条消息@了{entity.AllAt().Count()}次别人.").ConfigureAwait(false);
        }
        if (message.MessageEntity.RawString.Contains("qwq"))
        {
            await message.ReplyAsync($"你发了qwq是吧qwq, 你那条消息是在{message.SendTime.Value}的时候发的.").ConfigureAwait(false);
        }
        if (message.MessageEntity.Chain.AllAt().Any(n => n.User is null))
        {
            await message.Group.SendMessageAsync("刚才是不是有人@了一下全体成员...?").ConfigureAwait(false);
        }
        if (message.MessageEntity.RawString.Contains("临时虚拟一条加好友请求"))
        {
            if (client.PostSession is CqWebSocketSession ws)
            {
                string json = """
                    {
                        "post_type":"request",
                        "request_type":"friend",
                        "time":1670902817,
                        "self_id":2259113381,
                        "user_id":2748166392,
                        "comment":"",
                        "flag":"1670902816000000"
                    }
                    """;
                JsonDocument doc = JsonDocument.Parse(json);
                ws.EmitOnReceived(doc);
            }
        }
        //nothing
        Console.WriteLine();
    }

    private static void Client_OnGroupMemberDecreased(JoinedGroup group, User user)
    {
        logger.LogInfo("p", $"qwq, 群{group.Name.Value}里{user.Nickname.Value}走了");
    }

    private static void Client_OnGroupMemberChanged(JoinedGroup group, User user, bool isIncrease)
    {
        logger.LogInfo(
            "p", $"qwq, 群{group.Name.Value}里{user.Nickname.Value}这个人要么退群了要么加群了" +
            $", 答案是... {(isIncrease ? $"加群!(角色是:{user.Cast<GroupUser>().GroupRole.Value})" : "退群...")}"
            );
    }

    private static void Client_OnGroupMemberIncreased(JoinedGroup group, GroupUser user)
    {
        logger.LogInfo("p", $"awa, 群{group.Name.Value}里人了: {user.Nickname.Value}");
    }

    private static void ProcessMessageTest01(Message message)
    {
        /**
            if (rawString.Contains("/random"))
            {
                Random r = new();
                int num = r.Next(0, 100);
                await message.MessageWindow.SendMessageAsync($"{message.Sender.CqAt} {message.Sender.Nickname.Value},你的随机数为{num}哦~");
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
                    await message.MessageWindow.SendMessageAsync(new MessageEntity(cqEntity));
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
                    await message.MessageWindow.SendMessageAsync($"计算结果是: {rst}");
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

            if (rawString.Contains("狠狠骂我"))
            {
                var msg = await message.MessageWindow.SendMessageAsync("cnm, 有病吧");
                await Task.Delay(1000);
                await msg.RecallAsync();
                await message.MessageWindow.SendMessageAsync("qwq, 怎么能骂人呢awa");

            }

            //auto猜数游戏

            if (rawString.Contains("您猜了") && rawString.Contains("但是猜的数"))
            {
                const string currentRegion = "当前范围:";
                int loc = rawString.IndexOf(currentRegion);
                if (loc is not -1)
                {
                    string regionString = rawString[(loc + currentRegion.Length)..];
                    logger.LogInfo("Program", "猜数", $"区域字符串为: {regionString}");
                    string[] regions = regionString.Split("~");
                    long num1 = long.Parse(regions[0]);
                    long num2 = long.Parse(regions[1]);
                    long target = (num1 + num2) / 2;
                    logger.LogInfo("Program", "猜数", $"bot算出来{target}");
                    await Task.Delay(2000);
                    await message.MessageWindow.SendMessageAsync($"猜{target}");
                }
            }*/
    }

    private static void ProcessMesssageTest02(Message message)
    {
        /**
            if (message is GroupMessage groupMsg)
            {
                if (rawString.Contains("禁言"))
                {
                    var userId = (
                        from node in message.MessageEntity.CqEntity
                        where node is CqMessageAtNode
                        let atNode = node.Cast<CqMessageAtNode>()
                        select atNode.UserId
                        ).First();
                    await client.GetGroupUser(groupMsg.Group, userId).BanAsync(new TimeSpan(0, 1, 0));
                }
                if (rawString.Contains("解禁"))
                {
                    var userId = (
                        from node in message.MessageEntity.CqEntity
                        where node is CqMessageAtNode
                        let atNode = node.Cast<CqMessageAtNode>()
                        select atNode.UserId
                        ).First();
                    await client.GetGroupUser(groupMsg.Group, userId).LiftBanAsync();
                }
            }*/
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
                logger.LogDebug("Program", "RawPost", $"这是一条群消息, 群号是{gpost.GroupId}");
                string s = "/echo ";
                if (mpost.RawMessage.StartsWith(s))
                {
                    _ = client.SendGroupMessageAsync(gpost.GroupId, gpost.RawMessage[s.Length..]);
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
                    "unknown notify notice type")}",
                CqGroupMemberCardChangedNoticePost np8 =>
                    $"群{np8.GroupId}里的{np8.UserId}的名片从`{np8.CardOld}`变为了`{np8.CardNew}`",
                _ => $"未知的Notice接受,原始类型str是{np.NoticeType}"
            });
        }
        return;
    }

}