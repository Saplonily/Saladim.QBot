using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGroupAnonymousMessagePost : CQGroupMessagePost
{
    public long AnonymousUserId { get; protected set; } = -1;
    public string AnonymousUserName { get; protected set; } = string.Empty;
    public string AnonymousUserFlag { get; protected set; } = string.Empty;

    internal static CQGroupAnonymousMessagePost GetFrom(JsonElement je)
    {
        CQGroupAnonymousMessagePost post = new();
        post.LoadFrom(je);
        return post;
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            JsonElement anje = je.GetProperty("anonymous");
            AnonymousUserId = anje.GetProperty("id").GetInt64();
            AnonymousUserName = anje.GetProperty("name").GetString() ?? "";
            AnonymousUserFlag = anje.GetProperty("flag").GetString() ?? "";
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
        catch (FormatException ex)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), ex, $"from format exception: {ex.Message}");
        }
    }
}