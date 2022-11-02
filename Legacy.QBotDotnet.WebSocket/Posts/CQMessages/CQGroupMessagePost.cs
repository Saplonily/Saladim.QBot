using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGroupMessagePost : CQMessagePost
{
    public long GroupId { get; protected set; }
    internal new static CQGroupMessagePost GetFrom(JsonElement je, bool doUpdate)
    {
        CQGroupMessagePost post = new();
        post.LoadFrom(je);
        return !doUpdate ? post : post.MessageSubType switch
        {
            CQPostMessageSubType.Anonymous => CQGroupAnonymousMessagePost.GetFrom(je),
            _ => post
        };
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            GroupId = je.GetProperty("group_id").GetInt64();
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
        catch (FormatException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e, "The returned value is not in properly format.");
        }
    }
}