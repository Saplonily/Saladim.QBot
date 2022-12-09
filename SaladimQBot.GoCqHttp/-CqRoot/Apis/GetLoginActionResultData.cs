namespace SaladimQBot.GoCqHttp.Apis;

public class GetLoginActionResultData : CqApiCallResultData
{
    [Name("user_id")]
    public Int64 UserId { get; set; } = -1;

    [Name("nickname")]
    public string Nickname { get; set; } = default!;
}
