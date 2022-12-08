namespace SaladimQBot.GoCqHttp.Apis;

public class SendForwardMessageToGroupAction : CqApi
{
    [Name("group_id")]
    public long GroupId { get; set; } = -1;

    [Name("messages")]
    public ForwardEntityModel ForwardEntity { get; set; } = default!;

    public override string ApiName => "send_group_forward_msg";

    public override Type? ApiResultDataType => typeof(SendMessageActionResultData);
}
