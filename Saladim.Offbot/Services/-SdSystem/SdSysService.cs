using Saladim.Offbot.Entity;
using SaladimQBot.Extensions;
using SqlSugar;

namespace Saladim.Offbot.Services;

public class SdSysService
{
    protected SqlSugarScope sqlScope;

    public SdSysService(SqlSugarScope sqlScope)
    {
        this.sqlScope = sqlScope;
    }

    public UserSdEntity GetUserSdSession(long userId)
        => (sqlScope.Queryable<UserSdEntity>().Single(s => s.UserId == userId) ?? new() { UserId = userId });

    public long GetUserSd(long userId)
        => GetUserSdSession(userId).Sd;

    public void SetUserSd(long userId, long amount)
    {
        var se = GetUserSdSession(userId);
        se.Sd = amount;
        sqlScope.Storageable(se).ExecuteCommand();
    }

    public void UpdateUserSd(long userId, Func<long, long> func)
    {
        var se = GetUserSdSession(userId);
        se.Sd = func(se.Sd);
        sqlScope.Storageable(se).ExecuteCommand();
    }
}
