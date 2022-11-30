using System.Text.Json;
using System.Text.Json.Serialization;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class CqPostJsonConverter : JsonConverter<CqPost>
{
    public override CqPost? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        CqJsonPostLoader loader = new(doc.RootElement);
        var postType = loader.EnumFromString<CqPostType>(StringConsts.PostTypeProperty);
        Type? targetType = CqTypeMapper.FindClassForPostType(postType);
        if (targetType is null) return null;
        var updatedPost = JsonSerializer.Deserialize(doc, targetType, options);
        return updatedPost.AsCast<CqPost>();
    }

    public override void Write(Utf8JsonWriter writer, CqPost value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}