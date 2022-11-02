namespace QBotDotnet.GoCqHttp.Posts;

public class CqOtherMessagePost : CqMessagePost
{
    [Name("temp_source")]
    public MessageTempSource TempSource { get; set; }
}