namespace SaladimQBot.Extensions;

public class MemorySessionService
{
    protected Dictionary<Type, Dictionary<SessionId, ISession>> allSessions;

    public MemorySessionService()
    {
        allSessions = new();
    }

    public TSession GetSession<TSession>(SessionId sessionId) where TSession : class, ISession, new()
    {
        Type typeSession = typeof(TSession);
        if (allSessions.TryGetValue(typeSession, out var sessions))
        {
            if (sessions.TryGetValue(sessionId, out var session))
            {
                return (TSession)session;
            }
            else
            {
                lock (sessions)
                {
                    if (sessions.TryGetValue(sessionId, out var sessionAfterLock))
                    {
                        return (TSession)sessionAfterLock;
                    }
                    else
                    {
                        TSession newSession = new()
                        {
                            SessionId = sessionId
                        };
                        sessions.Add(sessionId, newSession);
                        return newSession;
                    }
                }
            }
        }
        else
        {
            lock (allSessions)
            {
                allSessions.TryAdd(typeSession, new());
            }
            return GetSession<TSession>(sessionId);
        }
    }

    public bool TryRemoveSession<TSession>(SessionId sessionId) where TSession : class, ISession, new()
    {
        Type typeSession = typeof(TSession);
        if (allSessions.TryGetValue(typeSession, out var sessions))
        {
            if (sessions.TryGetValue(sessionId, out var session))
            {
                sessions.Remove(session.SessionId);
                return true;
            }
        }
        return false;
    }

    public TSession GetUserSession<TSession>(long userId) where TSession : class, ISession, new()
        => GetSession<TSession>(new SessionId(userId));

    public TSession GetGroupSession<TSession>(long groupId) where TSession : class, ISession, new()
        => GetSession<TSession>(new SessionId(0, groupId));

    public TSession GetGroupUserSession<TSession>(long groupId, long userId) where TSession : class, ISession, new()
        => GetSession<TSession>(new SessionId(userId, groupId));

    public bool TryRemoveUserSession<TSession>(long userId) where TSession : class, ISession, new()
        => TryRemoveSession<TSession>(new SessionId(userId));

    public bool TryRemoveGroupSession<TSession>(long groupId) where TSession : class, ISession, new()
        => TryRemoveSession<TSession>(new SessionId(0, groupId));

    public bool TryRemoveGroupUserSession<TSession>(long groupId, long userId) where TSession : class, ISession, new()
        => TryRemoveSession<TSession>(new SessionId(userId, groupId));
}