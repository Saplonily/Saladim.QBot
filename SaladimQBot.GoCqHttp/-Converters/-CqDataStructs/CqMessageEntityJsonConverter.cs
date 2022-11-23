using System.Text.Json;
using System.Text.Json.Serialization;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

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
            //获取cq码类别
            CqCodeType type = MessageEntityNodeHelper.GetTypeFromString(
                item.GetProperty(StringConsts.CqCodeTypeProperty).GetString()!
                );
            //获取cq码对应的实体类
            Type? nodeClass = MessageEntityNodeHelper.FindClassFromCqCodeType(type);
            if (nodeClass is null) continue;

            object? node;
            if (type != CqCodeType.Unimplemented)
            {
                //不是未实现类别的cq码, 直接反序列化为cq码实体
                var dataProp = item.GetProperty(StringConsts.CqCodeParamsProperty);
                node = JsonSerializer.Deserialize(dataProp, nodeClass, options);
            }
            else
            {
                //未实现类别cq码, 使用上层的东西反序列化, type对应节点的Name属性
                //data对应Params属性
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
            writer.WriteStartObject();
            //cq码类别
            writer.WriteString(
                StringConsts.CqCodeTypeProperty,
                EnumAttributeCacher.GetAttrFromEnum<CqCodeType>(node.NodeType.Cast<int>()).Cast<string>()
                );
            writer.WritePropertyName(StringConsts.CqCodeParamsProperty);
            writer.WriteStartObject();
            //data
            foreach (var para in node.GetParamsDictionary())
                writer.WriteString(para.Key, para.Value);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}