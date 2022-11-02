using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;
public class CQGroupDecreaseNoticePost : CQNoticePost
{
    public long GroupId { get; protected set; }
    public long UserId { get; protected set; }
    public long OperatorId { get; protected set; }
    public WayToLeft Way { get; protected set; } = WayToLeft.Invalid;
    internal protected CQGroupDecreaseNoticePost()
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
                "leave" => WayToLeft.SelfLeave,
                "kick" => WayToLeft.Kick,
                "kick_me" => WayToLeft.KickSelf,
                _ => WayToLeft.Invalid
            };
            OperatorId = je.GetProperty("operator_id").GetInt64();
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    internal static CQGroupDecreaseNoticePost GetFrom(JsonElement je)
    {
        var post = new CQGroupDecreaseNoticePost();
        post.LoadFrom(je);
        return post;
    }

    public enum WayToLeft
    {
        Invalid,
        SelfLeave,
        Kick,
        KickSelf
    }
}