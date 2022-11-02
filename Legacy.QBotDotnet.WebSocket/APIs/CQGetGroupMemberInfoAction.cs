using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGetGroupMemberInfoAction : CQApi
{
    public override string ApiName { get => "get_group_member_info"; }

    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    public CQGetGroupMemberInfoAction(long groupId, long userId)
    {
        GroupId = groupId;
        UserId = userId;
    }
}