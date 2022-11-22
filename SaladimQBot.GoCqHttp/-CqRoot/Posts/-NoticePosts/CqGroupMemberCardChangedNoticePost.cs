namespace SaladimQBot.GoCqHttp.Posts;

public class CqGroupMemberCardChangedNoticePost : CqGroupUserNoticePost
{
    [Name("card_new")]
    public string CardNew { get; set; } = default!;

    [Name("card_old")]
    public string CardOld { get; set; } = default!;
}
