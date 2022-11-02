using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Posts;

public class CqGroupHonorChangedNotifyNoticePost : CqNotifyNoticePost
{
    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("honor_type")]
    public HonorSubType HonorType { get; set; }

    [JsonConverter(typeof(CqEnumJsonConverter))]
    public enum HonorSubType
    {
        Invalid,
        [NameIn("talkative")]
        Talkactive,
        [NameIn("performer")]
        Performer,
        [NameIn("emotion")]
        Emotion
    }
}