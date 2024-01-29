namespace SaladimQBot.GoCqHttp.Apis;

public class SetFriendAddRequestAction : CqApi
{
    public override string ApiName => "set_friend_add_request";

    public override Type? ApiResultDataType => null;

    [Name("flag")]
    public string Flag { get; set; } = default!;

    [Name("approve")]
    public bool IsApprove { get; set; } = false;
}
