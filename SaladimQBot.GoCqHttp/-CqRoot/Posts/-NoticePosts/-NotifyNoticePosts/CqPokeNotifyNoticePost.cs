namespace SaladimQBot.GoCqHttp.Posts;


public class CqPokeNotifyNoticePost : CqNotifyNoticePost
{
    [Name("sender_id")]
    public Int64? SenderId { get; set; }

    [Name("group_id")]
    public Int64? GroupId { get; set; }

    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("target_id")]
    public Int64 TargetId { get; set; }

    [Ignore]
    public bool InGroup { get => GroupId is not null; }
}
//别问,我也想知道为啥这俩重复了
//为了更好的封装...?