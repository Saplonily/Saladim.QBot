namespace SaladimQBot.GoCqHttp.Apis;

public class DeleteMessageAction : CqApi
{
    public override string ApiName => "delete_msg";

    public override Type? ApiResultDataType => typeof(EmptyActionResultData);

    [Name("message_id")]
    public int MessageId { get; set; }
}
