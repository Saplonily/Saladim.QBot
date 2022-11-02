namespace QBotDotnet.GoCqHttp.Posts;

public class CqNoticePost : CqPost
{
    [Name("notice_type")]
    public CqNoticeType NoticeType { get; set; }
}