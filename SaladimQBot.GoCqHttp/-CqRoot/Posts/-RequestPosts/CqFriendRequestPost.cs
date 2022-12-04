namespace SaladimQBot.GoCqHttp.Posts;

public class CqFriendRequestPost : CqRequestPost
{
    [Name("user_id")]
    public long UserId { get; set; } = -1;

    [Name("comment")]
    public string Comment { get; set; } = default!;

    [Name("flag")]
    public string Flag { get; set; } = default!;
}
