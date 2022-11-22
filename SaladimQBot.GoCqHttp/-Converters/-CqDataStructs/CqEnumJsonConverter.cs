using System.Text.Json;
using System.Text.Json.Serialization;

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
        => throw new NotSupportedException();

    public override bool CanConvert(Type typeToConvert)
    => typeof(Enum).IsAssignableFrom(typeToConvert);

}