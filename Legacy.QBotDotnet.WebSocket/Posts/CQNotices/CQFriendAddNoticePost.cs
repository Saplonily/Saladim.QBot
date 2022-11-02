using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQFriendAddNoticePost : CQNoticePost
{
    public long UserId { get; protected set; }
    internal static CQFriendAddNoticePost GetFrom(JsonElement rootJE)
    {
        CQFriendAddNoticePost post = new();
        post.LoadFrom(rootJE);
        return post;
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            UserId = je.GetProperty("user_id").GetInt64();
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
}