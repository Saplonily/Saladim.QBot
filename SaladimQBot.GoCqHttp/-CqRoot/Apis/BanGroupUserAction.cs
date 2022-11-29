namespace SaladimQBot.GoCqHttp.Apis;

public class BanGroupUserAction : CqApi
{
    public override string ApiName => "set_group_ban";

    public override Type? ApiResultDataType => null;

    [Name("user_id")]
    public long UserId { get; set; }

    [Name("group_id")]
    public long GroupId { get; set; }

    [Name("duration")]
    public int Duration { get; set; }
}
