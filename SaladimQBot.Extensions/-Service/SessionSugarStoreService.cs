using SqlSugar;

namespace SaladimQBot.Extensions;

public class SessionSugarStoreService
{
    protected SqlSugarScope sqlSugarScope;

    public SessionSugarStoreService(SqlSugarScope scope)
    {
        sqlSugarScope = scope;
    }

    public TSession GetSession<TSession>(SessionId sessionId) where TSession : SqliteStoreSession, new()
    {
        try
        {
            string sessionName = $"{sessionId.UserId},{sessionId.GroupId}";
            var foundResult = sqlSugarScope.Queryable<TSession>().Where(s => s.SessionString == sessionName).First();
            return foundResult ?? CreateNew();
        }
        catch (SqlSugarException e)
        {
            if (e.Message.Contains("no such table"))
            {
                sqlSugarScope.CodeFirst.InitTables<TSession>();
                return CreateNew();
            }
            else
            {
                throw;
            }
        }
        TSession CreateNew() => new() { SessionId = sessionId };
    }

    public void SaveSession<TSession>(TSession session) where TSession : class, ISession, new()
    {
        sqlSugarScope.Storageable(session).ExecuteCommand();
    }

    public ISugarQueryable<TSession> GetQueryable<TSession>() where TSession : SqliteStoreSession, new()
        => sqlSugarScope.Queryable<TSession>();

    public TSession GetUserSession<TSession>(long userId) where TSession : SqliteStoreSession, new()
        => GetSession<TSession>(new SessionId(userId));

    public TSession GetGroupSession<TSession>(long groupId) where TSession : SqliteStoreSession, new()
        => GetSession<TSession>(new SessionId(0, groupId));

    public TSession GetGroupUserSession<TSession>(long groupId, long userId) where TSession : SqliteStoreSession, new()
        => GetSession<TSession>(new SessionId(userId, groupId));
}

public abstract class SqliteStoreSession : ISession
{
    [SugarColumn(ColumnName = "session_string", IsPrimaryKey = true)]
    public string SessionString
    {
        get => $"{SessionId.UserId},{SessionId.GroupId}";
        set
        {
            var strs = value.Split(',');
            SessionId = new(long.Parse(strs[0]), long.Parse(strs[1]));
        }
    }

    [SugarColumn(IsIgnore = true)]
    public SessionId SessionId { get; set; }
}