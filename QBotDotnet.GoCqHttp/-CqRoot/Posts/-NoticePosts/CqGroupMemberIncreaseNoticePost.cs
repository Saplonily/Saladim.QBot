using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Posts;

public class CqGroupMemberIncreaseNoticePost : CqGroupUserOperatedNoticePost
{
    [Name("sub_type")]
    public NoticeSubType SubType { get; set; }

    [JsonConverter(typeof(CqEnumJsonConverter))]
    public enum NoticeSubType
    {
        Invalid,
        [NameIn("approve")]
        AdminApproved,
        [NameIn("invite")]
        Invited
    }
}