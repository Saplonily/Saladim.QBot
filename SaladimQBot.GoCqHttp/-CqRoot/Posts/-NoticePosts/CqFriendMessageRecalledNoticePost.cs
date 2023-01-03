namespace SaladimQBot.GoCqHttp.Posts;

public class CqPrivateMessageRecalledNoticePost : CqNoticePost
{
    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("message_id")]
    public Int32 MessageId { get; set; }
}