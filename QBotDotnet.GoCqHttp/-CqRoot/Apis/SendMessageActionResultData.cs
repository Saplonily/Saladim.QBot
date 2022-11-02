namespace QBotDotnet.GoCqHttp.Apis;

public class SendMessageActionResultData : CqApiCallResultData
{
    [property: Name("message_id")]
    public Int32 MessageId { get; set; }
}