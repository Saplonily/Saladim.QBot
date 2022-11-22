namespace SaladimQBot.GoCqHttp.Posts;

public class CqPrivateMessagePost : CqMessagePost
{
    [Name("temp_source")]

    public MessageTempSource TempSource { get; set; }
}