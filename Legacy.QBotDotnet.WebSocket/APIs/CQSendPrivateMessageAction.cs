using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQSendPrivateMessageAction : CQSendMessageAction
{
    public override string ApiName { get => "send_private_msg"; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }
    public CQSendPrivateMessageAction(long userId, CQMessage message, long groupId = -1)
    {
        UserId = userId;
        GroupId = groupId;
        Message = message;
    }
}