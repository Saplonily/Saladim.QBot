namespace SaladimQBot.GoCqHttp.Apis;

public class SetGroupCardAction : CqApi
{
    public override string ApiName => "set_group_card";

    public override Type? ApiResultDataType => null;

    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("card")]
    public string Card { get; set; } = null!;
}
