namespace SaladimQBot.GoCqHttp.Posts;

public class CqFriendAddedNoticePost : CqNoticePost
{
    [Name("user_id")]
    public Int64 UserId { get; set; }
}