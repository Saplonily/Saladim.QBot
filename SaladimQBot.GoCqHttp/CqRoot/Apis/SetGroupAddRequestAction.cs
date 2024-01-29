using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp.Apis;

public class SetGroupAddRequestAction : CqApi
{
    public override string ApiName => "set_group_add_request";

    public override Type? ApiResultDataType => null;

    [Name("sub_type")]
    public CqGroupRequestPost.RequestSubType SubType { get; set; }

    [Name("approve")]
    public bool IsApprove { get; set; }

    [Name("flag")]
    public string Flag { get; set; } = null!;

    [Name("reason")]
    public string? Reason { get; set; } = null;
}
