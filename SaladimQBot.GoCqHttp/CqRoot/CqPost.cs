namespace SaladimQBot.GoCqHttp;

public class CqPost
{
    [Name("time")]
    public long Time { get; set; } = default!;
    [Name("self_id")]
    public long SelfId { get; set; } = default!;
    [Name("post_type")]
    public CqPostType PostType { get; set; } = default!;
}