using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using System.Diagnostics.Metrics;

namespace SaladimWpf.Services;

public class ChessBoard
{
    /// <summary>
    /// 棋盘情况, 0表示空, 1到其余表示对应棋子
    /// </summary>
    private readonly int[,] board;

    public Func<int, Color> ChessPieceColorProvider { get; set; }

    public Color BgColor { get; set; } = new Color(new Rgba32(0xF1, 0xD6, 0xAB));

    public Color LineColor { get; set; } = new Color(new Rgba32(0x9D, 0x7E, 0x4F));

    public Color TextColor { get; set; } = Color.Gray;

    public int Rows { get; protected set; }

    public int Columns { get; protected set; }

    public int Width { get => Columns * GridSize; }

    public int Height { get => Rows * GridSize; }

    public int Count { get; protected set; }

    public int PlaceSucceedTimes { get; protected set; }

    public int GridSize { get; set; } = 64;

    public float ChessPieceSize { get; set; } = 0.8f * 64;

    public int HalfGridSize { get => GridSize / 2; }

    public ChessBoard(int rows, int columns, int count, Func<int, Color> chessPieceColorProvider)
    {
        Rows = rows;
        Columns = columns;
        ChessPieceColorProvider = chessPieceColorProvider;
        Count = count;
        board = new int[rows, columns];
    }

    public int GetAt(int row, int column)
        => board[row, column];

    public PointF GetPos(int row, int column)
        => new(HalfGridSize + column * GridSize, HalfGridSize + row * GridSize);

    public int PlaceAt(int row, int column, int what)
    {
        int before = GetAt(row, column);
        if (before == 0)
        {
            board[row, column] = what;
            PlaceSucceedTimes += 1;
            return 0;
        }
        else
        {
            return before;
        }
    }

    public Action<IImageProcessingContext> GetImageGenerateAction(Font textFont) => op =>
    {
        IPen linePen = new Pen(LineColor, 2);
        for (int row = 0; row < Rows; row++)
        {
            op.DrawLines(linePen, GetPos(row, 0), GetPos(row, Columns - 1));
        }
        for (int column = 0; column < Columns; column++)
        {
            op.DrawLines(linePen, GetPos(0, column), GetPos(Rows - 1, column));
        }
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                var curChess = GetAt(row, column);
                if (curChess != 0)
                {
                    IBrush brush = new SolidBrush(ChessPieceColorProvider(curChess));
                    PathBuilder b = new();
                    b.AddArc(GetPos(row, column), ChessPieceSize / 2, ChessPieceSize / 2, 0, 0, 360);
                    op.Fill(brush, b.Build());
                }
                else
                {
                    TextOptions o = new(textFont)
                    {
                        TextJustification = TextJustification.InterCharacter,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Origin = GetPos(row, column),
                        Dpi = 14
                    };
                    op.DrawText(o, $"({row},{column})", TextColor);
                }
            }
        }
    };

    public int CheckWinner()
    {
        int aim;
        var addCount = Count - 1;
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                aim = GetAt(r, c);
                if (aim == 0) continue;
                var win1 = true;
                var win2 = true;
                var win3 = true;
                var win4 = true;
                for (int i = 0; i < Count; i++)
                {
                    if (r + addCount >= Rows || GetAt(r + i, c) != aim)
                        win1 = false;
                    if (c + addCount >= Columns || GetAt(r, c + i) != aim)
                        win2 = false;
                    if (r + addCount >= Rows || c + addCount >= Columns || GetAt(r + i, c + i) != aim)
                        win3 = false;
                    if (r + addCount >= Rows || c - addCount < 0 || GetAt(r + i, c - i) != aim)
                        win4 = false;
                }
                if (win1 || win2 || win3 || win4)
                    return aim;
            }
        }
        return 0;
    }
}