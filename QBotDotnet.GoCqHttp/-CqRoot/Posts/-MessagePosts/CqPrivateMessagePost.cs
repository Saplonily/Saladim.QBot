using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Posts;

public class CqPrivateMessagePost : CqMessagePost
{
    [Name("temp_source")]

    public MessageTempSource TempSource { get; set; }
}