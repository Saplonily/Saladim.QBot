namespace SaladimQBot.GoCqHttp;

public class CqGroupTempMessageSender : CqMessageSender
{
    [Name("group_id")]
    public long GroupId { get; set; }
}
