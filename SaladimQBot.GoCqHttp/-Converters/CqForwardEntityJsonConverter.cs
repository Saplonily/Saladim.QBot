using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

public class CqForwardEntityJsonConverter : JsonConverter<ForwardEntity>
{
    public override ForwardEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ForwardEntity value, JsonSerializerOptions options)
    {

    }
}
