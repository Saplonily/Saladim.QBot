using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGroupRequestPost : CQRequestPost
{
    public CQGroupRequestSubType SubType { get; private set; } = CQGroupRequestSubType.Invalid;
    public long GroupId { get; private set; } = -1;
    public long UserId { get; private set; } = -1;
    public string Comment { get; private set; } = string.Empty;
    public string Flag { get; private set; } = string.Empty;
    internal new static CQGroupRequestPost GetFrom(JsonElement je, bool doUpdate)
    {
        CQGroupRequestPost post = new();
        post.LoadFrom(je);
        return !doUpdate ? post : post.SubType switch
        {
            CQGroupRequestSubType.Add => CQGroupAddRequestPost.GetFrom(je),
            CQGroupRequestSubType.Invite => CQGroupInviteRequestPost.GetFrom(je),
            _ => post
        }; ;
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            SubType = je.GetProperty("sub_type").GetString() switch
            {
                "add" => CQGroupRequestSubType.Add,
                "invite" => CQGroupRequestSubType.Invite,
                _ => CQGroupRequestSubType.Invalid
            };
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }

    public enum CQGroupRequestSubType
    {
        Invalid,
        Add,                          // 加群请求
        Invite                        // 被邀请请求
    }
}