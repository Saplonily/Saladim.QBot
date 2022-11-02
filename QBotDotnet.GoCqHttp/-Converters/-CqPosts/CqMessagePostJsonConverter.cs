using System.Text.Json;
using System.Text.Json.Serialization;
using QBotDotnet.GoCqHttp.Posts;
using QBotDotnet.SharedImplement;

namespace QBotDotnet.GoCqHttp;

public class CqMessagePostJsonConverter : JsonConverter<CqMessagePost>
{
    public override CqMessagePost? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        CqJsonPostLoader loader = new(doc.RootElement);
        var subType = loader.EnumFromString<CqMessageSubType>(StringConsts.MessagePostSubTypeProperty);
        var targetType = CqTypeMapper.FindClassFromCqMessagePostType(subType);
        if (targetType is null) return null;
        return JsonSerializer.Deserialize(doc, targetType, options).AsCast<CqMessagePost>();
    }


    public override void Write(Utf8JsonWriter writer, CqMessagePost value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}