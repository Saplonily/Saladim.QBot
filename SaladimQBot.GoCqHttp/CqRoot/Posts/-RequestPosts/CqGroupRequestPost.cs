using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp.Posts;

public class CqGroupRequestPost : CqRequestPost
{
    [Name("sub_type")]
    public RequestSubType SubType { get; set; } = RequestSubType.Invalid;

    [Name("group_id")]
    public long GroupId { get; set; } = -1;

    [Name("user_id")]
    public long UserId { get; set; } = -1;

    [Name("comment")]
    public string Comment { get; set; } = default!;

    [Name("flag")]
    public string Flag { get; set; } = default!;

    [JsonConverter(typeof(CqEnumJsonConverter))]
    public enum RequestSubType
    {
        Invalid,
        [NameIn("add")]
        Add,
        [NameIn("invite")]
        Invite
    }
}
