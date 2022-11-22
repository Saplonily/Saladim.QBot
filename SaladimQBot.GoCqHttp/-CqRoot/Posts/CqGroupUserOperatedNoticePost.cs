namespace SaladimQBot.GoCqHttp.Posts;

public abstract class CqGroupUserOperatedNoticePost : CqGroupUserNoticePost
{
    [Name("operator_id")]
    public Int64 OperatorId { get; set; }
}