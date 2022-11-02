namespace QBotDotnet.GoCqHttp.Apis;

public class SendGroupMessageActionResultData : CqApiCallResultData
{
    public SendGroupMessageActionResultData()
    {
    }
    public SendGroupMessageActionResultData(CqApiCallResult resultIn) : base(resultIn)
    {
    }

    [Name("message_id")]
    public int MessageId { get; set; }
}