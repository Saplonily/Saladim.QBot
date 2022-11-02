using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Internal;

public abstract class CQSendMessageAction : CQApi
{
    [JsonPropertyName("group_id")]
    public virtual long GroupId { get; set; }

    [JsonPropertyName("message")]
    public virtual CQMessage Message { get; set; } = null!;
}