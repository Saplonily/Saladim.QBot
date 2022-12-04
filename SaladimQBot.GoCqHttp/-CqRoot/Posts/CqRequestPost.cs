namespace SaladimQBot.GoCqHttp.Posts;

public class CqRequestPost : CqPost
{
    [Name("request_type")]
    public CqRequestType RequestType { get; set; }
}
