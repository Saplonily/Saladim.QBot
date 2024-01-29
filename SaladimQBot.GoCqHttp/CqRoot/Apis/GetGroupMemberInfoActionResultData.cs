namespace SaladimQBot.GoCqHttp.Apis;

public class GetGroupMemberInfoActionResultData : CqApiCallResultData
{
    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("nickname")]
    public string Nickname { get; set; } = default!;

    [Name("card")]
    public string Card { get; set; } = default!;

    [Name("sex")]
    public Sex Sex { get; set; }

    [Name("age")]
    public Int32 Age { get; set; }

    [Name("area")]
    public string Area { get; set; } = default!;

    [Name("join_time")]
    public Int64 JoinTime { get; set; }

    [Name("last_sent_time")]
    public Int64 LastSentTime { get; set; }

    [Name("level")]
    public string Level { get; set; } = default!;

    [Name("role")]
    public GroupRole Role { get; set; }

    [Name("unfriendly")]
    public bool Unfriendly { get; set; }

    [Name("title")]
    public string Title { get; set; } = default!;

    [Name("title_expire_time")]
    public Int64 TitleExpireTime { get; set; }

    [Name("card_changeable")]
    public bool CardChangeable { get; set; }

    [Name("shut_up_timestamp")]
    public Int64 ShutUpTimeStamp { get; set; }
}
