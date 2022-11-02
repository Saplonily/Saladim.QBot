using System.Text.Json;
using System.Text.Json.Serialization;
using QBotDotnet.SharedImplement;
using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;
public class CqMessageImageReceiveNodeJsonConverter : JsonConverter<CqMessageImageReceiveNode>
{
    public override CqMessageImageReceiveNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement ele = JsonDocument.ParseValue(ref reader).RootElement;
        if (!ele.TryGetProperty(CqMessageImageReceiveNode.FilePropertyName, out var fileEle)) return null;
        if (!ele.TryGetProperty(CqMessageImageReceiveNode.UrlPropertyName, out var urlEle)) return null;
        ele.TryGetProperty(CqMessageImageReceiveNode.SubTypePropertyName, out var subTypeEle);
        ele.TryGetProperty(CqMessageImageReceiveNode.ShowTypePropertyName, out var showTypeEle);
        ele.TryGetProperty(CqMessageImageReceiveNode.TypePropertyName, out var typeEle);

        string? file = fileEle.GetString(); if (file is null) return null;
        string? url = urlEle.GetString(); if (url is null) return null;
        //我也不知道我怎么把这玩意挤进一行里的
        ImageSendSubType subType =
            EnumAttributeCacher.GetEnumFromAttr<ImageSendSubType>(
                subTypeEle.ValueKind != JsonValueKind.Undefined ?
                (int.TryParse(subTypeEle.GetString(), out int result) ? result : -1) : -1
                ).Cast<ImageSendSubType>();

        //这个也是, 美观性max, 可读性吃屎
        ImageSendType sendType =
            EnumAttributeCacher.GetEnumFromAttr<ImageSendType>(
                typeEle.ValueKind != JsonValueKind.Undefined ?
                typeEle.GetString()! : -1
                ).Cast<ImageSendType>();

        ImageShowType showType =
            EnumAttributeCacher.GetEnumFromAttr<ImageShowType>(
                showTypeEle.ValueKind != JsonValueKind.Undefined ?
                (int.TryParse(showTypeEle.GetString()!, out int result2) ? result2 : -1) : -1
                ).Cast<ImageShowType>();
        return new CqMessageImageReceiveNode(
            url,
            file,
            (Core.ImageSendType)sendType,
            (Core.ImageSendSubType)subType,
            (Core.ImageShowType)showType
            );
    }

    public override void Write(Utf8JsonWriter writer, CqMessageImageReceiveNode value, JsonSerializerOptions options)
        => throw new NotSupportedException("Serialize a 'receive node' is not allowed.");
}