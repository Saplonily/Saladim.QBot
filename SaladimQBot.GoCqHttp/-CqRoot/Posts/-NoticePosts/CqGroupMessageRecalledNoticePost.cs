namespace SaladimQBot.GoCqHttp.Posts;

public class CqGroupMessageRecalledNoticePost : CqGroupUserOperatedNoticePost
{
    [Name("message_id")]
    public Int32 MessageId { get; set; }
}