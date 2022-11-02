using System.Text.Json;
using System.Text.Json.Serialization;
using QBotDotnet.GoCqHttp.Posts;
using QBotDotnet.SharedImplement;

namespace QBotDotnet.GoCqHttp;

public class CqNotifyNoticePostJsonConverter : JsonConverter<CqNotifyNoticePost>
{
    public override CqNotifyNoticePost? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement root = JsonDocument.ParseValue(ref reader).RootElement;
        CqJsonPostLoader loader = new(root);
        var notifyType = loader.EnumFromString<CqNotifySubType>(StringConsts.NotifySubTypeProperty);
        var targetType = notifyType switch
        {
            CqNotifySubType.GroupHonorChanged => typeof(CqGroupHonorChangedNotifyNoticePost),
            CqNotifySubType.Poke => typeof(CqPokeNotifyNoticePost),
            CqNotifySubType.LuckyKing => typeof(CqLuckyKingNotifyNoticePost),
            _ => null
        };
        if (targetType is null) return null;
        return JsonSerializer.Deserialize(root, targetType, options).AsCast<CqNotifyNoticePost>();
    }

    public override void Write(Utf8JsonWriter writer, CqNotifyNoticePost value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}