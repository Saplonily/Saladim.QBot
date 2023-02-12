using SqlSugar;

namespace Saladim.Offbot.Entity;

[SugarTable("sd_sys")]
public class UserSdEntity
{
    [SugarColumn(ColumnName = "user_id", IsPrimaryKey = true)]
    public long UserId { get; set; }

    [SugarColumn(ColumnName = "sd")]
    public long Sd { get; set; }


}
