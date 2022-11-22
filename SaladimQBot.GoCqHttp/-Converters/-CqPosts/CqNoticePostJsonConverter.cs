using System.Text.Json;
using System.Text.Json.Serialization;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class CqNoticePostJsonConverter : JsonConverter<CqNoticePost>
{
    public override CqNoticePost? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement root = JsonDocument.ParseValue(ref reader).RootElement;
        CqJsonPostLoader loader = new(root);
        CqNoticeType noticeType = loader.EnumFromString<CqNoticeType>(StringConsts.NoticeTypeProperty);
        var typeToUpdate = CqTypeMapper.FindClassForCqNoticeType(noticeType);
        if (typeToUpdate is null) return null;
        return JsonSerializer.Deserialize(root, typeToUpdate, options).AsCast<CqNoticePost>();
    }

    public override void Write(Utf8JsonWriter writer, CqNoticePost value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}