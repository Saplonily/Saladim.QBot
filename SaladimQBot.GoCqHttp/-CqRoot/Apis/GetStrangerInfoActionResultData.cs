namespace SaladimQBot.GoCqHttp.Apis;
public class GetStrangerInfoActionResultData : CqApiCallResultData
{
    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("nickname")]
    public string Nickname { get; set; } = default!;

    [Name("sex")]
    public Sex Sex { get; set; }

    [Name("age")]
    public Int32 Age { get; set; }

    [Name("qid")]
    public string Qid { get; set; } = default!;

    [Name("level")]
    public Int32 Level { get; set; }

    [Name("login_days")]
    public Int32 LoginDays { get; set; }
}
