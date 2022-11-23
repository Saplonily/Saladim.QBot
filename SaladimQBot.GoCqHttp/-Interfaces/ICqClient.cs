using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public interface ICqClient : IClient, IExpirableValueGetter
{
    ICqSession ApiSession { get; }

    ICqSession PostSession { get; }

    TimeSpan ExpireTimeSpan { get; }

    Task<CqApiCallResult?> CallApiAsync(CqApi api);

    Task<GroupMessage> SendGroupMessageAsync(long groupId, MessageEntity messageEntity);

    new Task<GroupMessage> SendGroupMessageAsync(long groupId, string rawString);

    Task<PrivateMessage> SendPrivateMessageAsync(long userId, MessageEntity messageEntity);

    new Task<PrivateMessage> SendPrivateMessageAsync(long userId, string rawString);
}
