using System.Text.Json;
using System.Text.Json.Serialization;
using QBotDotnet.SharedImplement;

namespace QBotDotnet.GoCqHttp;

/*
    [
        {
            "type": "at",
            "data": {
                "qq": 123456
            }
        },
        {
            "type": "text",
            "data": {
                "text": "早上好啊"
            }
        }
    ]
*/
public class CqMessageEntityJsonConverter : JsonConverter<CqMessageEntity>
{
    public override CqMessageEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Array) return null;

        CqMessageEntity entity = new();
        foreach (var item in root.EnumerateArray())
        {
            CqCodeType type = MessageEntityNodeHelper.GetTypeFromString(
                item.GetProperty(StringConsts.CqCodeTypeProperty).GetString()!
                );

            Type? nodeClass = MessageEntityNodeHelper.FindClassFromCqCodeType(type);
            if (nodeClass is null) continue;

            object? node;
            if (nodeClass != typeof(CqMessageUnimplementedNode))
            {
                var dataProp = item.GetProperty(StringConsts.CqCodeParamsProperty);
                node = JsonSerializer.Deserialize(dataProp, nodeClass, options);
            }
            else
            {
                node = JsonSerializer.Deserialize<CqMessageUnimplementedNode>(item, options);
            }

            if (node is null) continue;

            entity.Add(node.Cast<CqMessageEntityNode>());
        }
        return entity;
    }

    public override void Write(Utf8JsonWriter writer, CqMessageEntity value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var node in value)
        {
            if (node is not CqMessageUnimplementedNode unimplNode)
            {
                writer.WriteStartObject();
                writer.WriteString(
                    StringConsts.CqCodeTypeProperty,
                    EnumAttributeCacher.GetAttrFromEnum<CqCodeType>(node.NodeType.Cast<int>()).Cast<string>()
                    );
                writer.WritePropertyName(StringConsts.CqCodeParamsProperty);
                JsonSerializer.Serialize(writer, node, node.GetType(), options);
                writer.WriteEndObject();
            }
            else
            {
                JsonSerializer.Serialize(writer, unimplNode, options);
            }
        }
        writer.WriteEndArray();
    }
}