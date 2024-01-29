using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp.Posts;

public class CqGroupEssenceSetNoticePost : CqNoticePost
{
    [Name("sender_id")]
    public Int64 SenderId { get; set; }

    [Name("operator_id")]
    public Int64 OperatorId { get; set; }

    [Name("message_id")]
    public Int32 MessageId { get; set; }

    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("sub_type")]
    public NoticeSubType SubType { get; set; }

    [JsonConverter(typeof(CqEnumJsonConverter))]
    public enum NoticeSubType
    {
        Invalid,
        [NameIn("add")]
        Add,
        [NameIn("delete")]
        Delete
    }
}