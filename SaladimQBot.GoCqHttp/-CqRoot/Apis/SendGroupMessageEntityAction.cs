namespace SaladimQBot.GoCqHttp.Apis;

public class SendGroupMessageEntityAction : CqApi
{
    public override string ApiName => "send_msg";

    public override Type ApiResultDataType => typeof(SendMessageActionResultData);

    [Name("group_id")]
    public long GroupId { get; set; }

    [Name("message")]
    public CqMessageChainModel Message { get; set; } = default!;
}