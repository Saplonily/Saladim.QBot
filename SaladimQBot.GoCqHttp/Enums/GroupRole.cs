using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

[JsonConverter(typeof(CqEnumJsonConverter))]
public enum GroupRole
{
    Invalid,
    [NameIn("owner")]
    Owner,
    [NameIn("admin")]
    Admin,
    [NameIn("member")]
    Member
}
