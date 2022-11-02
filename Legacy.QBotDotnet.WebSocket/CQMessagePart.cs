using System.Text.Json;
using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp;

public class CQMessagePart
{
    [JsonIgnore]
    internal CQPartType Type { get; private set; } = CQPartType.Invalid;

    [JsonPropertyName("data")]
    public Dictionary<string, string> CQData { get; private set; } = new Dictionary<string, string>();

    [JsonPropertyName("type")]
    public string CQTypeInString
    {
        get => Type switch
        {
            CQPartType.Text => "text",
            CQPartType.Face => "face",
            CQPartType.At => "at",
            CQPartType.Share => "share",
            CQPartType.Music => "music",
            _ => "invalid"
        };
    }

    internal CQMessagePart(CQPartType cqType, Dictionary<string, string> cqData)
    {
        Type = cqType;
        CQData = cqData;
    }

    internal static CQMessagePart GetFrom(JsonElement je)
    {
        try
        {
            CQPartType cqType = je.GetProperty("type").GetString() switch
            {
                "text" => CQPartType.Text,
                "face" => CQPartType.Face,
                "at" => CQPartType.At,
                "share" => CQPartType.Share,
                "music" => CQPartType.Music,
                _ => CQPartType.Invalid
            };
            Dictionary<string, string> dataDic = new();
            JsonElement dataJE = je.GetProperty("data");
            foreach (var obj in dataJE.EnumerateObject())
            {
                dataDic.Add(obj.Name, obj.Value.GetString() ?? string.Empty);
            }

            CQMessagePart part = new(cqType, dataDic);
            return part;
        }
        catch (KeyNotFoundException e)
        {
            throw new Posts.CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    public enum CQPartType
    {
        Invalid,
        Text,
        Face,
        At,
        Share,
        Music
    }
}
