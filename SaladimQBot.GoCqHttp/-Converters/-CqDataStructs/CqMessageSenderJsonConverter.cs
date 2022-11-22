using System.Text.Json;
using System.Text.Json.Serialization;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class CqMessageSenderJsonConverter : JsonConverter<CqMessageSender>
{
    public override CqMessageSender? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);

        var sender = JsonSerializer.Deserialize<CqMessageSender>(doc, options);

        return doc.RootElement.ExistsProperty(StringConsts.GroupSenderIdentifier) ?
            JsonSerializer.Deserialize<CqGroupMessageSender>(doc, options) : sender;
    }

    public override void Write(Utf8JsonWriter writer, CqMessageSender value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}