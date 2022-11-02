using System.Text.Json.Serialization;
using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp.Posts;

public class CqMessagePost : CqPost
{
    [Name("sub_type")]
    public CqMessageSubType SubType { get; set; }

    [Name("message_id")]
    public Int32 MessageId { get; set; }

    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("message")]
    public CqMessageEntity MessageEntity { get; set; } = default!;

    [Name("raw_message")]
    public string RawMessage { get; set; } = default!;

    [Name("font")]
    public Int32 Font { get; set; }

    [Name("sender")]
    [JsonConverter(typeof(CqMessageSenderJsonConverter))]
    public CqMessageSender Sender { get; set; } = default!;
}
