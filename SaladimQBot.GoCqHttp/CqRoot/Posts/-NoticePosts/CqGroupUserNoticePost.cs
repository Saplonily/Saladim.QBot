namespace SaladimQBot.GoCqHttp.Posts;

public abstract class CqGroupUserNoticePost : CqNoticePost
{
    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("group_id")]
    public Int64 GroupId { get; set; }
}