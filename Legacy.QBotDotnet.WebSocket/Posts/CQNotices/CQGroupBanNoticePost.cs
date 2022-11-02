using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;
public class CQGroupBanNoticePost : CQNoticePost
{
    public long GroupId { get; protected set; } = -1;
    public long UserId { get; protected set; } = -1;
    public long OperatorId { get; protected set; } = -1;
    /// <summary>
    /// 禁言时长，单位为秒
    /// </summary>
    public long Duration { get; protected set; }
    public bool IsLiftBan { get; protected set; }
    internal protected CQGroupBanNoticePost() { }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        GroupNoticeHelper.LoadFrom(je, out var groupId, out var userId);
        GroupId = groupId; UserId = userId;
        try
        {
            OperatorId = je.GetProperty("operator_id").GetInt64();
            Duration = je.GetProperty("duration").GetInt64();
            IsLiftBan = je.GetProperty("sub_type").GetString() switch
            {
                "ban" => false,
                "lift_ban" => true,
                _ => throw new CQPostTypeInvalidLoadException(nameof(je), text: "group ban notice -> sub_type is none of [ban,lift_ban]")
            };
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    internal static CQGroupBanNoticePost GetFrom(JsonElement je)
    {
        var post = new CQGroupBanNoticePost();
        post.LoadFrom(je);
        return post;
    }
}