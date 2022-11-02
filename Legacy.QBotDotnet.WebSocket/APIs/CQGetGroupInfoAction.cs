using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGetGroupInfoAction : CQApi
{
    public override string ApiName { get => "get_group_info"; }

    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    public CQGetGroupInfoAction(long groupId)
    {
        GroupId = groupId;
    }
}