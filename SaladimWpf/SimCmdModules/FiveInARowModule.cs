using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;
using SaladimQBot.Core;
using SaladimQBot.Extensions;
using SaladimQBot.Shared;
using SaladimWpf.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SQLite;
using Point = System.Drawing.Point;

namespace SaladimWpf.SimCmdModules;

public class FiveInARowModule : CommandModule
{
    public const string TipMsgGroupOnly = "五子棋游戏只在群聊中进行哦~";
    public const string TipMsgAlreadyGaming = "你已经在进行游戏啦~";
    public const string TipMsgWaitingNextStart = "正在等待下一个开始五子棋的玩家... 等待中或游戏中可使用 /退出五子棋 来强制结束游戏.";
    public const string TipMsgGameStarted = "五子棋已经开始啦, 输入 /下 1,5 这种格式的指令下第一子吧!";
    public const string TipMsgWaitingNextStartTwice = "正在等待下一个开始五子棋的玩家...请稍安勿躁qwq";
    public const string TipMsgPieceBlocked = "那里有人下过了哦, 再想想哦.";
    public const string TipMsgNotPlaying = "您还未进入正在进行的游戏中哦~ 输入 /开始五子棋 等待加入吧!";
    public const string TipMsgOutOfRange = "位置越界了哦, 请输入一个正确的坐标.";
    protected readonly FiveInARowService fiveInARowService;
    protected readonly MemorySessionService memorySessionService;
    protected readonly CoroutineService coroutineService;
    protected readonly SessionSqliteService sessionSqliteService;

    public FiveInARowModule(
        FiveInARowService fiveInARowService,
        MemorySessionService memorySessionService,
        CoroutineService coroutineService,
        SessionSqliteService sessionSqliteService
        )
    {
        this.fiveInARowService = fiveInARowService;
        this.memorySessionService = memorySessionService;
        this.coroutineService = coroutineService;
        this.sessionSqliteService = sessionSqliteService;
    }

    [Command("开始五子棋")]
    public void StartNew()
    {
        if (Content.Message is IGroupMessage groupMessage)
        {
            if (!fiveInARowService.IsPlayerPlaying(groupMessage.Sender))
            {
                var s = memorySessionService.GetGroupSession<LineUpSession>(groupMessage.Group.GroupId);
                lock (s)
                {
                    if (s.CurrentWaitings.Count >= 1)
                    {
                        if (!s.CurrentWaitings[0].IsSameGroupUser(groupMessage.Sender))
                        {
                            var gameRecord = fiveInARowService.CreateAndAddNewGame(s.CurrentWaitings[0], groupMessage.Sender, 17, 17, 5);
                            var co = GetGameCoroutine(gameRecord);
                            coroutineService.AddNewCoroutine(co);
                            foreach (var user in gameRecord.Users)
                            {
                                var gamingSession = memorySessionService.GetUserSession<GamingSession>(user.UserId);
                                gamingSession.FiveInARowCorotine = co;
                            }
                            memorySessionService.TryRemoveGroupSession<LineUpSession>(groupMessage.Group.GroupId);
                            Content.MessageWindow.SendMessageAsync(TipMsgGameStarted);
                        }
                        else
                        {
                            Content.MessageWindow.SendMessageAsync(TipMsgWaitingNextStartTwice);
                        }
                    }
                    else
                    {
                        s.CurrentWaitings.Add(groupMessage.Sender);
                        Content.MessageWindow.SendMessageAsync(TipMsgWaitingNextStart);
                    }
                }
            }
            else
            {
                FiveInARowRecord record = fiveInARowService.GetPlayerPlaying(groupMessage.Sender);
                StringBuilder sb = new();
                sb.AppendLine(TipMsgAlreadyGaming);
                sb.Append($"你是在这个群开始游戏的: {record.Users[0].Group.Name}");
                sb.Append($"({record.Users[0].Group.GroupId})");
                Content.MessageWindow.SendMessageAsync(Content.Client.CreateTextOnlyEntity(sb.ToString()));
            }
        }
        else
        {
            Content.MessageWindow.SendMessageAsync(TipMsgGroupOnly);
        }
    }

    [Command("退出五子棋")]
    public void EndNow()
    {
        if (Content.Message is IGroupMessage groupMessage)
        {
            if (!fiveInARowService.IsPlayerPlaying(groupMessage.Sender))
            {
                var s = memorySessionService.GetGroupSession<LineUpSession>(groupMessage.Group.GroupId);
                if (s.CurrentWaitings.Any(u => u.IsSameGroupUser((IGroupUser)Content.Executor)))
                {
                    Content.MessageWindow.SendMessageAsync("已退出五子棋等待.");
                    memorySessionService.TryRemoveGroupSession<LineUpSession>(groupMessage.Group.GroupId);
                }
                else
                {
                    Content.MessageWindow.SendMessageAsync("你既没有等待中也没有游玩中哦.");
                }
            }
            else
            {
                var playingOne = fiveInARowService.GetPlayerPlaying(groupMessage.Sender);
                if (playingOne.ChessBoard.PlaceSucceedTimes >= 5)
                {
                    Content.MessageWindow.SendMessageAsync("游戏已结束, 但是已经下了5个子以上了, 认为你认输了哦qwq");
                    var s = sessionSqliteService.GetUserSession<FiveInARowWinnerSession>(groupMessage.Sender.UserId);
                    s.LoseTimes += 1;
                    sessionSqliteService.SaveSession(s);
                }
                else
                {
                    Content.MessageWindow.SendMessageAsync("游戏已结束, 还未下够5个子, 没有成绩记录了哦.");
                }
                fiveInARowService.EndGame(playingOne);
                var session = memorySessionService.GetUserSession<GamingSession>(Content.Executor.UserId);
                var co = session.FiveInARowCorotine!;
                coroutineService.RemoveCoroutine(co);
            }
        }
        else
        {
            Content.MessageWindow.SendMessageAsync(TipMsgGroupOnly);
        }
    }

    public IEnumerator<EventWaiter> GetGameCoroutine(FiveInARowRecord record)
    {
        IMessageEntity startMsgEntity = Content.Client.CreateMessageBuilder()
            .WithImage($"file:///{GenerateChessImage(record.ChessBoard)}")
            .WithText("\n")
            .WithAt(record.Users[0])
            .WithText("从你开始咯!")
            .Build();
        Content.MessageWindow.SendMessageAsync(startMsgEntity);
        int curPlayer = 0;
        while (true)
        {
            object[] args = null!;
            yield return new CommandWaiter(Content.SimCommandExecuter, record.Users[curPlayer], "下", report => args = report);
            Vector2 pos = args[0].Cast<Vector2>();
            Point point = new((int)pos.X, (int)pos.Y);
            if (point.X >= 0 && point.X < record.ChessBoard.Columns && point.Y >= 0 && point.Y < record.ChessBoard.Rows)
            {
                int at = record.ChessBoard.PlaceAt(point.X, point.Y, curPlayer + 1);
                if (at != 0)
                {
                    Content.MessageWindow.SendMessageAsync(TipMsgPieceBlocked);
                }
                else
                {
                    int winner = record.ChessBoard.CheckWinner();
                    if (winner == 0)
                    {
                        curPlayer += 1;
                        if (record.Users.Count == curPlayer)
                            curPlayer = 0;
                        IMessageEntity entity = Content.Client.CreateMessageBuilder()
                            .WithImage($"file:///{GenerateChessImage(record.ChessBoard)}")
                            .WithText("\n")
                            .WithAt(record.Users[curPlayer])
                            .WithText("到你了!")
                            .Build();
                        Content.MessageWindow.SendMessageAsync(entity);
                    }
                    else
                    {
                        //有人赢了
                        IGroupUser winnerUser = record.Users[winner - 1];
                        IMessageEntity entity = Content.Client.CreateMessageBuilder()
                            .WithImage($"file:///{GenerateChessImage(record.ChessBoard)}")
                            .WithText("\n恭喜")
                            .WithAt(winnerUser)
                            .WithText("赢得了本次游戏的胜利!")
                            .Build();
                        Content.MessageWindow.SendMessageAsync(entity);
                        fiveInARowService.EndGame(record);
                        foreach (var u in record.Users)
                        {
                            var s = sessionSqliteService.GetUserSession<FiveInARowWinnerSession>(u.UserId);
                            if (u.IsSameUser(winnerUser))
                                s.WinTimes += 1;
                            else
                                s.LoseTimes += 1;
                            sessionSqliteService.SaveSession(s);
                        }
                        yield break;
                    }
                }
            }
            else
            {
                Content.MessageWindow.SendMessageAsync(TipMsgOutOfRange);
            }
        }
    }

    protected string GenerateChessImage(ChessBoard chessBoard)
    {
        Image<Rgba32> image = new(chessBoard.Width, chessBoard.Height, chessBoard.BgColor);
        image.Mutate(chessBoard.GetImageGenerateAction(fiveInARowService.ChessPieceTextFont));
        string fileName = $@"tempImages\{DateTime.Now.Ticks}.png";
        if (!Directory.Exists("tempImages"))
            Directory.CreateDirectory("tempImages");
        image.SaveAsPng(fileName);
        return Path.GetFullPath(fileName);
    }

    [Command("下")]
    public void Drop(Vector2 pos) => GamePlayCheck();

    public void GamePlayCheck()
    {
        if (Content.Executor is IGroupUser groupUser)
        {
            if (!fiveInARowService.IsPlayerPlaying(groupUser))
            {
                Content.MessageWindow.SendMessageAsync(TipMsgNotPlaying);
            }
        }
        else
        {
            Content.MessageWindow.SendMessageAsync(TipMsgGroupOnly);
        }
    }

    public class GamingSession : ISession
    {
        public SessionId SessionId { get; set; }

        public IEnumerator<EventWaiter>? FiveInARowCorotine { get; set; }
    }

    public class LineUpSession : ISession
    {
        public SessionId SessionId { get; set; }

        public List<IGroupUser> CurrentWaitings { get; set; } = new();
    }

    [Table("fiveInARowWinner")]
    public class FiveInARowWinnerSession : SqliteStoreSession
    {
        [Column("winTimes")]
        public int WinTimes { get; set; }

        [Column("loseTimes")]
        public int LoseTimes { get; set; }
    }
}
