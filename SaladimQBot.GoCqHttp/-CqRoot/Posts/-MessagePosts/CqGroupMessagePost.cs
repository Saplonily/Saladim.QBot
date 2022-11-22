namespace SaladimQBot.GoCqHttp.Posts;

public class CqGroupMessagePost : CqMessagePost
{
    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("anonymous")]
    public CqGroupAnonymousSender? AnonymousSender { get; set; }
}