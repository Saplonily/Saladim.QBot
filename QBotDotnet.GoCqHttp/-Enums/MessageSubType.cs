using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp;

[JsonConverter(typeof(CqEnumJsonConverter))]
public enum CqMessageSubType
{
    Invalid,
    [NameIn("friend")]
    Friend,
    [NameIn("group")]
    TempFromGroup,
    [NameIn("group_self")]
    GroupFromSelf,
    [NameIn("normal")]
    Group,
    [NameIn("anonymous")]
    Anonymous,
    [NameIn("other")]
    Other
}