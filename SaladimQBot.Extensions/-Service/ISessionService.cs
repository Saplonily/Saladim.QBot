using System;

namespace SaladimQBot.Extensions;

public interface ISessionService
{
    TSession GetSession<TSession>(SessionId sessionId) where TSession : class, ISession, new();

    void SaveSession<TSession>(TSession session) where TSession : class, ISession, new();
}

public static class ISessionServiceExtensions
{
    public static TSession GetUserSession<TSession>(this ISessionService service, long userId)
        where TSession : class, ISession, new()
        => service.GetSession<TSession>(new SessionId(userId));

    public static TSession GetGroupSession<TSession>(this ISessionService service, long groupId)
        where TSession : class, ISession, new()
    => service.GetSession<TSession>(new SessionId(0, groupId));

    public static TSession GetGroupUserSession<TSession>(this ISessionService service, long groupId, long userId)
        where TSession : class, ISession, new()
    => service.GetSession<TSession>(new SessionId(userId, groupId));
}

public interface ISession
{
    SessionId SessionId { get; set; }
}