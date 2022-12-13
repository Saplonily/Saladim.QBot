using System.Text.Json;
using System.Text.Json.Serialization;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class CqEnumJsonConverter : JsonConverter<Enum>
{
    public override Enum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int rawValue = 0;
        try
        {
            rawValue = EnumAttributeCacher.GetEnumFromAttr(
                typeToConvert,
                reader.TokenType == JsonTokenType.Number ? reader.GetInt32() : reader.GetString()!
                );
        }
        catch (KeyNotFoundException) { }
        return (Enum)Enum.ToObject(typeToConvert, rawValue);
    }

    public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
    {
        object v = EnumAttributeCacher.GetAttrFromEnum(value.GetType(), value.Cast<int>());
        if (v is string)
        {
            writer.WriteStringValue(v.Cast<string>());
        }
        if (v is int)
        {
            writer.WriteNumberValue(v.Cast<int>());
        }
    }

    public override bool CanConvert(Type typeToConvert)
        => typeof(Enum).IsAssignableFrom(typeToConvert);

}