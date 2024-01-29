using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

[JsonConverter(typeof(CqEnumJsonConverter))]
public enum Sex
{
    Invalid,
    [NameIn("unknown")]
    Unknown,
    [NameIn("male")]
    Male,
    [NameIn("female")]
    Female
}
