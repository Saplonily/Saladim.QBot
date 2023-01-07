using SaladimQBot.Core;
using SaladimQBot.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SaladimWpf.Services;

public class FiveInARowService
{
    protected List<FiveInARowRecord> playingGames = new();
    protected readonly CoroutineService coroutineService;

    public IReadOnlyList<FiveInARowRecord> PlayingGames { get => playingGames; }

    public Font ChessPieceTextFont { get; protected set; }

    public FiveInARowService(CoroutineService coroutineService)
    {
        FontCollection collection = new();
        collection.AddSystemFonts();
        FontFamily family = collection.Get("SimSun");
        ChessPieceTextFont = new(family, 96, FontStyle.Italic);
        this.coroutineService = coroutineService;
    }

    public bool IsPlayerPlaying(IGroupUser user)
        => PlayingGames.FirstOrDefault(g => g.Users.Any(p => p.IsSameUser(user))) is not null;

    public bool IsGroupPlaying(IJoinedGroup group)
        => PlayingGames.FirstOrDefault(g => g.Users.Any(u => u.Group.IsSameGroup(group))) is not null;

    public FiveInARowRecord GetPlayerPlaying(IGroupUser user)
        => PlayingGames.First(r => r.Users.Any(u => u.IsSameUser(user)));

    public FiveInARowRecord CreateAndAddNewGame(IGroupUser user1, IGroupUser user2, int rows, int columns, int count)
    {
        FiveInARowRecord r = new(new() { user1, user2 }, new(rows, columns, count, GetChessPieceColor));
        playingGames.Add(r);
        return r;
    }

    public void EndGame(FiveInARowRecord record)
    {
        playingGames.Remove(record);
    }

    public static Color GetChessPieceColor(int i) => i switch
    {
        1 => new Color(new Rgba32(0x2A, 0x30, 0x30)),
        2 => new Color(new Rgba32(0xEF, 0xEF, 0xEF)),
        _ => Color.Black
    };
}

public record FiveInARowRecord(List<IGroupUser> Users, ChessBoard ChessBoard);