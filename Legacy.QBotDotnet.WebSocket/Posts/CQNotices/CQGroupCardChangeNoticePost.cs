using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;
public class CQGroupCardChangeNoticePost : CQNoticePost
{
    public long GroupId { get; protected set; }
    public long UserId { get; protected set; }
    public string CardOld { get; protected set; } = string.Empty;
    public string CardNew { get; protected set; } = string.Empty;
    internal protected CQGroupCardChangeNoticePost() { }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        GroupNoticeHelper.LoadFrom(je, out var groupId, out var userId);
        GroupId = groupId; UserId = userId;
        try
        {
            CardOld = je.GetProperty("card_old").GetString() ?? "";
            CardNew = je.GetProperty("card_new").GetString() ?? "";
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    internal static CQGroupCardChangeNoticePost GetFrom(JsonElement je)
    {
        var post = new CQGroupCardChangeNoticePost();
        post.LoadFrom(je);
        return post;
    }
}