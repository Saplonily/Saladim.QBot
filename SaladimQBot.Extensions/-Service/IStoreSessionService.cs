using System;

namespace SaladimQBot.Extensions;

public interface IStoreSessionService
{
    TSession GetSession<TSession>(SessionId sessionId) where TSession : class, ISession, new();

    void SaveSession<TSession>(TSession session) where TSession : class, ISession, new();
}

public static class ISessionServiceExtensions
{
    public static TSession GetUserSession<TSession>(this IStoreSessionService service, long userId)
        where TSession : class, ISession, new()
        => service.GetSession<TSession>(new SessionId(userId));

    public static TSession GetGroupSession<TSession>(this IStoreSessionService service, long groupId)
        where TSession : class, ISession, new()
    => service.GetSession<TSession>(new SessionId(0, groupId));

    public static TSession GetGroupUserSession<TSession>(this IStoreSessionService service, long groupId, long userId)
        where TSession : class, ISession, new()
    => service.GetSession<TSession>(new SessionId(userId, groupId));
}
