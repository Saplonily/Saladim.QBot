using SaladimQBot.Extensions;
using SqlSugar;

namespace Saladim.Offbot.Services;

public class SdSysService
{
    protected SessionSugarStoreService sss;
    protected SqlSugarScope sugarScope;

    public SdSysService(SessionSugarStoreService sessionSqliteService, SqlSugarScope sugarScope)
    {
        this.sss = sessionSqliteService;
        this.sugarScope = sugarScope;
    }

    public long GetUserSd(long userId)
        => sss.GetUserSession<UserSdSession>(userId).Sd;

    public void SetUserSd(long userId, long amount)
    {
        var s = sss.GetUserSession<UserSdSession>(userId);
        s.Sd = amount;
        sss.SaveSession(s);
    }
}

[SugarTable("sd_sys")]
public class UserSdSession : SugarStoreSession
{
    [SugarColumn(ColumnName = "sd")]
    public long Sd { get; set; }


}

[SugarTable("sealed_sd")]
public class SealedSds
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true)]
    public int Id { get; set; }
}