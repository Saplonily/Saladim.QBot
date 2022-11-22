namespace SaladimQBot.GoCqHttp.Posts;
public class CqTitleChangedNotifyNoticePost : CqNotifyNoticePost
{
    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("title")]
    public string Title { get; set; } = default!;
}
