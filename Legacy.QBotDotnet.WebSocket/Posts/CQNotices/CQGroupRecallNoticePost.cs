using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;
public class CQGroupRecallNoticePost : CQNoticePost
{
    public long GroupId { get; protected set; }
    public long UserId { get; protected set; }
    public long OperatorId { get; protected set; }
    public long MessageId { get; protected set; }
    internal protected CQGroupRecallNoticePost() { }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        GroupNoticeHelper.LoadFrom(je, out var groupId, out var userId);
        GroupId = groupId; UserId = userId;
        try
        {
            OperatorId = je.GetProperty("operator_id").GetInt64();
            MessageId = je.GetProperty("message_id").GetInt64();
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    internal static CQGroupRecallNoticePost GetFrom(JsonElement je)
    {
        var post = new CQGroupRecallNoticePost();
        post.LoadFrom(je);
        return post;
    }
}