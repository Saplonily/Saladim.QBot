using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGroupAddRequestPost : CQGroupRequestPost
{
    internal static CQGroupAddRequestPost GetFrom(JsonElement je)
    {
        CQGroupAddRequestPost post = new();
        post.LoadFrom(je);
        return post;
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {

        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
}