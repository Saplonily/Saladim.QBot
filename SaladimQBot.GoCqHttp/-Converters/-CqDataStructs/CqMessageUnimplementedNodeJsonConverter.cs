using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

public class CqMessageUnimplementedNodeJsonConverter : JsonConverter<CqMessageUnimplementedNode>
{
    public override CqMessageUnimplementedNode? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
        )
    {
        JsonElement root = JsonDocument.ParseValue(ref reader).RootElement;
        //获取cq码类型
        string? typeStr = root.GetProperty(StringConsts.CqCodeTypeProperty).GetString();
        if (typeStr is null) return null;
        //获取cq码的data对象
        JsonElement paramsProperty = root.GetProperty(StringConsts.CqCodeParamsProperty);
        //反序列化到一个字符串字典
        var strDic = paramsProperty.Deserialize<StringDictionary>(options);
        if (strDic is null) return null;
        //new对象, 返回
        return new(typeStr, strDic);
    }

    public override void Write(Utf8JsonWriter writer,
        CqMessageUnimplementedNode value,
        JsonSerializerOptions options
        )
        => throw new NotSupportedException("No MessageNode serialization action allowed");
}