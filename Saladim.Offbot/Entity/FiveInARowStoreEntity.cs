using SqlSugar;

namespace Saladim.Offbot.Entity;


[SugarTable("five_in_a_row")]
public class FiveInARowStoreEntity
{
    [SugarColumn(ColumnName = "user_id", IsPrimaryKey = true)]
    public long UserId { get; set; }

    [SugarColumn(ColumnName = "win_times")]
    public int WinTimes { get; set; }

    [SugarColumn(ColumnName = "lose_times")]
    public int LoseTimes { get; set; }
}