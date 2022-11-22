using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

[JsonConverter(typeof(CqEnumJsonConverter))]
public enum CqNotifySubType
{
    Invalid,
    [NameIn("honor")]
    GroupHonorChanged,
    [NameIn("poke")]
    Poke,
    [NameIn("lucky_king")]
    LuckyKing,
    [NameIn("title")]
    Title
}