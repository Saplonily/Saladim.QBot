using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp;

[JsonConverter(typeof(CqEnumJsonConverter))]
public enum CqNotifySubType
{
    Invalid,
    [NameIn("honor")]
    GroupHonorChanged,
    [NameIn("poke")]
    Poke,
    [NameIn("lucky_king")]
    LuckyKing
}