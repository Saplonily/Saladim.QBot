using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Posts;

public class CqGroupAdminChangedNoticePost : CqGroupUserNoticePost
{
    [Name("sub_type")]
    public NoticeSubType SubType { get; set; }

    [JsonConverter(typeof(CqEnumJsonConverter))]
    public enum NoticeSubType
    {
        Invalid,
        [NameIn("set")]
        Set,
        [NameIn("unset")]
        Cancel
    }
}