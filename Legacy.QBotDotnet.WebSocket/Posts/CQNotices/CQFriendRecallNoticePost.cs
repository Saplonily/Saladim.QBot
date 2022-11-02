using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;
public class CQFriendRecallNoticePost : CQNoticePost
{
    public long GroupId { get; protected set; }
    public long UserId { get; protected set; }
    public long MessageId { get; protected set; }
    internal protected CQFriendRecallNoticePost() { }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        GroupNoticeHelper.LoadFrom(je, out var groupId, out var userId);
        GroupId = groupId; UserId = userId;
        try
        {
            MessageId = je.GetProperty("message_id").GetInt64();
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    internal static CQFriendRecallNoticePost GetFrom(JsonElement je)
    {
        var post = new CQFriendRecallNoticePost();
        post.LoadFrom(je);
        return post;
    }
}