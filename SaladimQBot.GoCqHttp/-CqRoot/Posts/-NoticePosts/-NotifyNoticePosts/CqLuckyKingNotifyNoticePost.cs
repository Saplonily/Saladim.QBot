namespace SaladimQBot.GoCqHttp.Posts;

public class CqLuckyKingNotifyNoticePost : CqNotifyNoticePost
{
    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("user_id")]
    public Int64 SenderId { get; set; }

    [Name("target_id")]
    public Int64 UserId { get; set; }
}