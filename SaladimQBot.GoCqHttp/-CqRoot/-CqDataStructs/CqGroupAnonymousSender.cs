namespace SaladimQBot.GoCqHttp;

public class CqGroupAnonymousSender
{
    [Name("id")]
    public Int64 Id { get; set; }

    [Name("name")]
    public string Name { get; set; } = default!;

    [Name("flag")]
    public string Flag { get; set; } = default!;
}