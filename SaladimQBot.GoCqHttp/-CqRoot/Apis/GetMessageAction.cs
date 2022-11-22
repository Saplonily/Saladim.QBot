namespace SaladimQBot.GoCqHttp.Apis;

public class GetMessageAction : CqApi
{
    public override string ApiName => "get_msg";

    public override Type? ApiResultDataType => typeof(GetMessageActionResultData);

    [Name("message_id")]
    public long MessageId { get; set; }
}