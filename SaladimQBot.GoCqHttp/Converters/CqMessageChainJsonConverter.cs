using System.Text.Json;
using System.Text.Json.Serialization;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

internal class CqMessageChainModelJsonConverter : JsonConverter<CqMessageChainModel>
{
    public override CqMessageChainModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement root = JsonDocument.ParseValue(ref reader).RootElement;
        if (root.ValueKind != JsonValueKind.Array) return null;

        CqMessageChainModel entity = new();
        foreach (var chainNode in root.EnumerateArray())
        {
            CqMessageChainNodeModel? node =
                JsonSerializer.Deserialize<CqMessageChainNodeModel>(chainNode, CqJsonOptions.Instance);
            if (node is not null)
                node.RawCqCodeName = chainNode.GetProperty(StringConsts.CqCodeTypeProperty).GetString();
            if (node is null) continue;
            entity.ChainNodeModels.Add(node);
        }
        return entity;
    }

    public override void Write(Utf8JsonWriter writer, CqMessageChainModel value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var node in value.ChainNodeModels)
        {
            writer.WriteStartObject();

            //cq码类别
            string cqCodeTypeString = node.CqCodeType != CqCodeType.Unimplemented ?
                EnumAttributeCacher.GetAttrFromEnum<CqCodeType>(node.NodeType.Cast<int>()).Cast<string>() :
                node.RawCqCodeName ??
                throw new InvalidOperationException(
                    "CqMessageChainNodeModel is a unimplemented one, " +
                    "but without RawCqCodeName"
                );
            writer.WriteString(StringConsts.CqCodeTypeProperty, cqCodeTypeString);
            writer.WritePropertyName(StringConsts.CqCodeParamsProperty);
            writer.WriteStartObject();
            //data
            foreach (var para in node.Params)
                writer.WriteString(para.Key, para.Value);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    public static CqCodeType GetCqTypeFromString(string name)
    {
        try
        {
            return EnumAttributeCacher.GetEnumFromAttr<CqCodeType>(name);
        }
        catch (KeyNotFoundException)
        {
            return CqCodeType.Unimplemented;
        }
    }
}

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