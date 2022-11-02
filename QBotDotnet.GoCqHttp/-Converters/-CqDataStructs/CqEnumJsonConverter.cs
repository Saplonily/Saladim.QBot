using System.Text.Json;
using System.Text.Json.Serialization;
using QBotDotnet.SharedImplement;

namespace QBotDotnet.GoCqHttp;

public class CqEnumJsonConverter : JsonConverter<Enum>
{
    public override Enum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var v = EnumAttributeCacher.GetEnumFromAttr(
            typeToConvert,
            reader.TokenType == JsonTokenType.Number ? reader.GetInt32() : reader.GetString()!
            );
        return (Enum)Enum.ToObject(typeToConvert, v);
    }

    public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
        => throw new NotSupportedException();

    public override bool CanConvert(Type typeToConvert)
    => typeof(Enum).IsAssignableFrom(typeToConvert);

}