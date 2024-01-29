using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

[JsonConverter(typeof(CqEnumJsonConverter))]
public enum CqRequestType
{
    Invalid,
    [NameIn("friend")]
    Friend,
    [NameIn("group")]
    Group
}
