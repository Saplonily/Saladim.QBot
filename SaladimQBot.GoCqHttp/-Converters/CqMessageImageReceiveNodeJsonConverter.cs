using System.Text.Json;
using System.Text.Json.Serialization;
using SaladimQBot.Shared;


namespace SaladimQBot.GoCqHttp;
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

        int rawValue = subTypeEle.ValueKind != JsonValueKind.Undefined ?
                (int.TryParse(subTypeEle.GetString(), out int result) ? result : -1) : -1;

        //实际收到上报时会有114514种未定义的ImageSendSubType
        //这里如果尝试获取其他未定义的SubType的话赋值ImageSendSubType.Invalid
        ImageSendSubType subType;
        try
        {
            subType = EnumAttributeCacher.GetEnumFromAttr<ImageSendSubType>(rawValue)
                .Cast<ImageSendSubType>();
        }
        catch (KeyNotFoundException)
        {
            subType = ImageSendSubType.Invalid;
        }

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
            sendType,
            subType,
            showType
            );
    }

    public override void Write(Utf8JsonWriter writer, CqMessageImageReceiveNode value, JsonSerializerOptions options)
        => throw new NotSupportedException("Serialize a 'receive node' is not allowed.");
}