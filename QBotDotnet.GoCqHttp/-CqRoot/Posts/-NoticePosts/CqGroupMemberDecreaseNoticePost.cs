using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Posts;

public class CqGroupMemberDecreaseNoticePost : CqGroupUserOperatedNoticePost
{
    [Name("sub_type")]
    public NoticeSubType SubType { get; set; }

    [JsonConverter(typeof(CqEnumJsonConverter))]
    public enum NoticeSubType
    {
        Invalid,
        [NameIn("leave")]
        ActivelyLeave,
        [NameIn("kick")]
        BeKicked,
        [NameIn("kick_me")]
        SelfBeKicked
    }
}