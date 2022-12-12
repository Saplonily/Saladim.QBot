namespace SaladimQBot.GoCqHttp.Apis;

public class SendPrivateMessageEntityAction : CqApi
{
    public override string ApiName { get => "send_private_msg"; }

    public override Type ApiResultDataType { get => typeof(SendMessageActionResultData); }

    [Name("message")]
    public CqMessageChainModel Message { get; set; } = default!;

    [Name("user_id")]
    public long UserId { get; set; } = -1;

    [Name("group_id")]
    public long? GroupId { get; set; } = null;
}
