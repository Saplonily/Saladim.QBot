using System.Reflection;
using SQLite;

namespace SaladimQBot.Extensions;

public class SessionSqliteService : IStoreSessionService
{
    protected SQLiteConnection sqliteConnection;

    public SessionSqliteService(SessionSqliteServiceConfig config)
    {
        sqliteConnection = new(config.SQLiteConnectionString);
    }

    public TSession GetSession<TSession>(SessionId sessionId) where TSession : ISession, new()
    {
        lock (sqliteConnection)
        {
            if (!typeof(SqliteStoreSession).IsAssignableFrom(typeof(TSession)))
                throw new InvalidOperationException("The session type must subclass SqliteSession.");
            try
            {
                var foundResult = sqliteConnection.Find<TSession>($"{sessionId.UserId},{sessionId.GroupId}");
                return foundResult ?? CreateNew();
            }
            catch (SQLiteException e)
            {
                if (e.Message.Contains("no such table"))
                {
                    sqliteConnection.CreateTable<TSession>();
                    return CreateNew();

                }
                else
                {
                    throw;
                }
            }
        }
        TSession CreateNew() => new() { SessionId = sessionId };
    }

    public void SaveSession<TSession>(TSession session)
    {
        sqliteConnection.InsertOrReplace(session);
    }

    TSession IStoreSessionService.GetSession<TSession>(SessionId sessionId)
        => this.GetSession<TSession>(sessionId);

    void IStoreSessionService.SaveSession<TSession>(TSession session)
        => this.SaveSession(session);
}

public abstract class SqliteStoreSession : ISession
{
    [PrimaryKey, Column("session_string")]
    public string SessionString
    {
        get => $"{SessionId.UserId},{SessionId.GroupId}";
        set
        {
            var strs = value.Split(',');
            SessionId = new(long.Parse(strs[0]), long.Parse(strs[1]));
        }
    }

    [Ignore]
    public SessionId SessionId { get; set; }
}

public class SessionSqliteServiceConfig
{
    public SQLiteConnectionString SQLiteConnectionString { get; set; }

    public SessionSqliteServiceConfig(SQLiteConnectionString sqliteConnectionString)
    {
        this.SQLiteConnectionString = sqliteConnectionString;
    }
}