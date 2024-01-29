namespace SaladimQBot.GoCqHttp.Apis;

public class SendForwardMessageToUserAction : CqApi
{
    public override string ApiName => "send_private_forward_msg";

    public override Type? ApiResultDataType => typeof(SendMessageActionResultData);

    [Name("user_id")]
    public long UserId { get; set; } = -1;

    [Name("messages")]
    public ForwardEntityModel ForwardEntity { get; set; } = null!;
}
