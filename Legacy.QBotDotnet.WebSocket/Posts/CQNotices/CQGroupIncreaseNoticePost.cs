using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;
public class CQGroupIncreaseNoticePost : CQNoticePost
{
    public long GroupId { get; protected set; } = -1;
    public long UserId { get; protected set; } = -1;
    public long OperatorId { get; protected set; } = -1;
    public WayToJoin Way { get; protected set; } = WayToJoin.Invalid;
    internal protected CQGroupIncreaseNoticePost()
    { }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        GroupNoticeHelper.LoadFrom(je, out var groupId, out var userId);
        GroupId = groupId; UserId = userId;
        try
        {
            Way = je.GetProperty("sub_type").ToString() switch
            {
                "approve" => WayToJoin.AdminApprove,
                "invite" => WayToJoin.AdminInvite,
                _ => WayToJoin.Invalid
            };
            OperatorId = je.GetProperty("operator_id").GetInt64();
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    internal static CQGroupIncreaseNoticePost GetFrom(JsonElement je)
    {
        var post = new CQGroupIncreaseNoticePost();
        post.LoadFrom(je);
        return post;
    }

    public enum WayToJoin
    {
        Invalid,
        AdminApprove,
        AdminInvite
    }
}