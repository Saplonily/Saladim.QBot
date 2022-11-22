using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public interface ICqClient : IClient, IExpirableValueGetter
{
    ICqSession ApiSession { get; }

    ICqSession PostSession { get; }

    TimeSpan ExpireTimeSpan { get; }

    Task<CqApiCallResult?> CallApiAsync(CqApi api);
}
