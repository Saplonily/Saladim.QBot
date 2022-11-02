using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQRequestPost : CQPost
{
    public CQRequestType RequestType { get; set; } = CQRequestType.Invalid;
    internal new static CQRequestPost GetFrom(JsonElement je, bool doUpdate)
    {
        CQRequestPost post = new();
        post.LoadFrom(je);
        return !doUpdate ? post : post.RequestType switch
        {
            CQRequestType.Friend => CQFriendRequestPost.GetFrom(je),
            CQRequestType.Group => CQGroupRequestPost.GetFrom(je, doUpdate),
            _ => post
        };
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            RequestType = je.GetProperty("request_type").GetString() switch
            {
                "friend" => CQRequestType.Friend,
                "group" => CQRequestType.Group,
                _ => CQRequestType.Invalid
            };
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }

    public enum CQRequestType
    {
        Invalid,
        Friend,
        Group
    }
}