namespace SaladimQBot.GoCqHttp.Apis;

public class GetForwardMessageAction : CqApi
{
    public override string ApiName => "get_forward_msg";

    public override Type? ApiResultDataType => typeof(GetForwardMessageActionResultData);

    [Name("id")]
    public string ForwardId { get; set; } = default!;
}
