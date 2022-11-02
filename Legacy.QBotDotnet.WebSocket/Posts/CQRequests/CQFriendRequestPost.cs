using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQFriendRequestPost : CQRequestPost
{
    public long UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
    internal static CQFriendRequestPost GetFrom(JsonElement je)
    {
        CQFriendRequestPost post = new();
        post.LoadFrom(je);
        return post;
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            UserId = je.GetProperty("user_id").GetInt64();
            Comment = je.GetProperty("comment").GetString() ?? "";
            Flag = je.GetProperty("flag").GetString() ?? "";
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
}