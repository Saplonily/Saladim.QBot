namespace QBotDotnet.GoCqHttp.Posts;

public class CqGroupMessageRecalledNoticePost : CqGroupUserOperatedNoticePost
{
    [Name("message_id")]
    public Int64 MessageId { get; set; }
}