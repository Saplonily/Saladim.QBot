using System.Text.Json;
using System.Text.Json.Serialization;
using QBotDotnet.SharedImplement;

namespace QBotDotnet.GoCqHttp;

public class CqMessageUnimplementedNodeJsonConverter : JsonConverter<CqMessageUnimplementedNode>
{
    public override CqMessageUnimplementedNode? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
        )
    {
        JsonElement root = JsonDocument.ParseValue(ref reader).RootElement;
        string? typeStr = root.GetProperty(StringConsts.CqCodeTypeProperty).GetString();
        if (typeStr is null) return null;
        JsonElement paramsProperty = root.GetProperty(StringConsts.CqCodeParamsProperty);
        var strDic = paramsProperty.Deserialize<StringDictionary>(options);
        if (strDic is null) return null;
        CqMessageUnimplementedNode node = new(typeStr, strDic);
        return node;
    }

    public override void Write(Utf8JsonWriter writer,
        CqMessageUnimplementedNode value,
        JsonSerializerOptions options
        )
    {
        writer.WriteStartObject();
        writer.WriteString(StringConsts.CqCodeTypeProperty, value.Name);
        writer.WritePropertyName(StringConsts.CqCodeParamsProperty);
        writer.WriteStartObject();
        foreach (var pair in value.Params)
        {
            writer.WriteString(pair.Key, pair.Value);
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}