using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using Saladim.Offbot.Services;
using SaladimQBot.Core;
using SaladimQBot.Extensions;
using SaladimQBot.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SqlSugar;
using Point = System.Drawing.Point;

namespace Saladim.Offbot.SimCmdModules;

public class FiveInARowModule : CommandModule
{
    public const string TipMsgWined = "赢得了本次游戏的胜利!";
    public const string TipMsgTurnYour = "到你了!";
    public const string TipMsgGroupOnly = "五子棋游戏只在群聊中进行哦~";
    public const string TipMsgOutOfRange = "位置越界了哦, 请输入一个正确的坐标.";
    public const string TipMsgNotPlaying = "您还未进入正在进行的游戏中哦~ 输入 /开始五子棋 等待加入吧!";
    public const string TipMsgGameStarted = "五子棋已经开始啦, 输入 /下 1,5 这种格式的指令下第一子吧!";
    public const string TipMsgStartFromYou = "从你开始咯!";
    public const string TipMsgPieceBlocked = "那里有人下过了哦, 再想想哦.";
    public const string TipMsgExitedWaiting = "已退出五子棋等待.";
    public const string TipMsgAlreadyGaming = "你已经在进行游戏啦~";
    public const string TipMsgRadioHighscore = "赢输比例排行榜:";
    public const string TipMsgWinnerHighscore = "赢局排行榜:";
    public const string TipMsgCongratulations = "恭喜";
    public const string TipMsgWaitingNextStart = "正在等待下一个开始五子棋的玩家... 等待中或游戏中可使用 /退出五子棋 来强制结束游戏.";
    public const string TipMsgGamePlayEndOver5 = "游戏已结束, 但是已经下了5个子以上了, 认为你认输了哦qwq";
    public const string TipMsgGamePlayEndLower5 = "游戏已结束, 还未下够5个子, 没有成绩记录了哦.";
    public const string TipMsgRadioHighscoreTip = "比例:";
    public const string TipMsgHasGamePlayingInGroup = "你是在这个群开始游戏的:";
    public const string TipMsgWaitingNextStartTwice = "正在等待下一个开始五子棋的玩家...请稍安勿躁qwq";
    public const string TipMsgNoneWaitingNonePlaying = "你既没有等待中也没有游玩中哦.";
    public const string TipMsgGroupAlreadyGamePlaying = "该群已有人进行游戏了哦";

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
            if (!fiveInARowService.IsGroupPlaying(groupMessage.Group))
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
                    sb.Append($"{TipMsgHasGamePlayingInGroup} {record.Users[0].Group.Name}");
                    sb.Append($"({record.Users[0].Group.GroupId})");
                    Content.MessageWindow.SendMessageAsync(Content.Client.CreateTextOnlyEntity(sb.ToString()));
                }
            }
            else
            {
                Content.MessageWindow.SendMessageAsync(TipMsgGroupAlreadyGamePlaying);
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
                    Content.MessageWindow.SendMessageAsync(TipMsgExitedWaiting);
                    memorySessionService.TryRemoveGroupSession<LineUpSession>(groupMessage.Group.GroupId);
                }
                else
                {
                    Content.MessageWindow.SendMessageAsync(TipMsgNoneWaitingNonePlaying);
                }
            }
            else
            {
                var playingOne = fiveInARowService.GetPlayerPlaying(groupMessage.Sender);
                if (playingOne.ChessBoard.PlaceSucceedTimes >= 5)
                {
                    Content.MessageWindow.SendMessageAsync(TipMsgGamePlayEndOver5);
                    var s = sessionSqliteService.GetUserSession<FiveInARowStoreSession>(groupMessage.Sender.UserId);
                    s.LoseTimes += 1;
                    sessionSqliteService.SaveSession(s);
                }
                else
                {
                    Content.MessageWindow.SendMessageAsync(TipMsgGamePlayEndLower5);
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

    [Command("五子棋排行")]
    public void HighScores()
    {
        var winHighScores = sessionSqliteService
            .GetQueryable<FiveInARowStoreSession>()
            .OrderByDescending(s => s.WinTimes)
            .Take(3)
            .ToList();

        var radioHighScores = sessionSqliteService
            .GetQueryable<FiveInARowStoreSession>()
            .Where(s => s.LoseTimes != 0)
            .OrderByDescending(s => (float)s.WinTimes / s.LoseTimes)
            .ToList();

        StringBuilder sb = new();
        int cur = 0;
        sb.AppendLine(TipMsgWinnerHighscore);
        foreach (var winHighScore in winHighScores)
        {
            cur++;
            sb.AppendLine($"{cur}. {Content.Client.GetUser(winHighScore.SessionId.UserId).Nickname} {winHighScore.WinTimes}次");
        }
        sb.AppendLine(TipMsgRadioHighscore);
        cur = 0;
        foreach (var radioHignScore in radioHighScores)
        {
            cur++;
            string str = $"{cur}. {Content.Client.GetUser(radioHignScore.SessionId.UserId).Nickname} " +
                $"{TipMsgRadioHighscoreTip} {(float)radioHignScore.WinTimes / radioHignScore.LoseTimes:F2}";
            sb.AppendLine(str);
        }
        Content.MessageWindow.SendTextMessageAsync(sb.ToString());
    }

    public IEnumerator<EventWaiter> GetGameCoroutine(FiveInARowRecord record)
    {
        IMessageEntity startMsgEntity = Content.Client.CreateMessageBuilder()
            .WithImage($"file:///{GenerateChessImage(record.ChessBoard)}")
            .WithText("\n")
            .WithAt(record.Users[0])
            .WithText(TipMsgStartFromYou)
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
                            .WithText(TipMsgTurnYour)
                            .Build();
                        Content.MessageWindow.SendMessageAsync(entity);
                    }
                    else
                    {
                        //有人赢了
                        IGroupUser winnerUser = record.Users[winner - 1];
                        IMessageEntity entity = Content.Client.CreateMessageBuilder()
                            .WithImage($"file:///{GenerateChessImage(record.ChessBoard)}")
                            .WithText($"\n{TipMsgCongratulations}")
                            .WithAt(winnerUser)
                            .WithText(TipMsgWined)
                            .Build();
                        Content.MessageWindow.SendMessageAsync(entity);
                        fiveInARowService.EndGame(record);
                        foreach (var u in record.Users)
                        {
                            var s = sessionSqliteService.GetUserSession<FiveInARowStoreSession>(u.UserId);
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
    [SuppressMessage("Style", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
    public void Drop(Vector2 pos) => GamePlayCheck();

    public void GamePlayCheck()
    {
        if (Content.Executor is IGroupUser groupUser)
        {
            if (!fiveInARowService.IsPlayerPlaying(groupUser))
                Content.MessageWindow.SendMessageAsync(TipMsgNotPlaying);
        }
        else
            Content.MessageWindow.SendMessageAsync(TipMsgGroupOnly);
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

    [SugarTable("five_in_a_row")]
    public class FiveInARowStoreSession : SqliteStoreSession
    {
        [SugarColumn(ColumnName = "win_times")]
        public int WinTimes { get; set; }

        [SugarColumn(ColumnName = "lose_times")]
        public int LoseTimes { get; set; }
    }
}
