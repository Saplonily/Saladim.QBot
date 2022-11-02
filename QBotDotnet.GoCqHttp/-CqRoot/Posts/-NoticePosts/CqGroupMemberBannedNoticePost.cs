using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Posts;

public class CqGroupMemberBannedNoticePost : CqGroupUserOperatedNoticePost
{
    /// <summary>
    /// 禁言时长，单位秒
    /// </summary>
    [Name("duration")]
    public Int64 Duration { get; set; }

    [Name("sub_type")]
    public NoticeSubType SubType { get; set; }

    [JsonConverter(typeof(CqEnumJsonConverter))]
    public enum NoticeSubType
    {
        Invalid,
        [NameIn("ban")]
        Ban,
        [NameIn("lift_ban")]
        LiftBan
    }
}