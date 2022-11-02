using System.Text.Json;
using System.Text.Json.Serialization;
using QBotDotnet.GoCqHttp.Posts;

namespace QBotDotnet.GoCqHttp;

public class CqPostJsonConverter : JsonConverter<CqPost>
{
    public override CqPost? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        CqJsonPostLoader loader = new(doc.RootElement);
        var postType = loader.EnumFromString<CqPostType>(StringConsts.PostTypeProperty);

        CqPost? post = null;
        var updatedPost = postType switch
        {
            CqPostType.Message => JsonSerializer.Deserialize<CqMessagePost?>(doc, options),
            CqPostType.Request => post,
            CqPostType.Notice => JsonSerializer.Deserialize<CqNoticePost?>(doc, options),
            CqPostType.MetaEvent => post,
            _ => post
        };

        return updatedPost;
    }

    public override void Write(Utf8JsonWriter writer, CqPost value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}