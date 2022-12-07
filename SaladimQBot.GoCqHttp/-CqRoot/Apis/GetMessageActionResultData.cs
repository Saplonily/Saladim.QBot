

using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp.Apis;

public class GetMessageActionResultData : CqApiCallResultData
{
    [Name("message")]
    public CqMessageChainModel MessageEntity { get; set; } = default!;

    [Name("message_id")]
    public long MessageId { get; set; }

    [Name("group")]
    public bool IsGroupMessage { get; set; }

    [Name("group_id")]
    public long? GroupId { get; set; } = null;

    [Name("sender")]
    public SenderOfGottenMessage Sender { get; set; } = default!;

    public class SenderOfGottenMessage
    {
        [Name("nickname")]
        public string Nickname { get; set; } = default!;

        [Name("user_id")]
        public long UserId { get; set; }
    }

    [JsonConverter(typeof(CqEnumJsonConverter))]
    public enum MessageType
    {
        Invalid,
        [NameIn("group")]
        Group,
        [NameIn("private")]
        Private
    }
}
