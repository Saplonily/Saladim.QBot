using SqlSugar;

namespace Saladim.Offbot.Entity;

[SugarTable("sealed_sd")]
public class SealedSdsEntity
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
}