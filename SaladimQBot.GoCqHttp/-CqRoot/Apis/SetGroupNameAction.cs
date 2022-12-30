namespace SaladimQBot.GoCqHttp.Apis;

public class SetGroupNameAction : CqApi
{
    public override string ApiName => "set_group_name";

    public override Type? ApiResultDataType => null;

    [Name("group_id")]
    public long GroupId { get; set; }

    [Name("group_name")]
    public string GroupName { get; set; } = null!;
}
