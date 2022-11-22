using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

/// <summary>
/// <para><see cref="CqCodeType"/>类与<see cref="Core.MessageNodeType"/>枚举高度联系</para>
/// <para>修改两者之一枚举请注意同步修改防止强转出错</para>
/// </summary>
[JsonConverter(typeof(CqEnumJsonConverter))]
public enum CqCodeType
{
    Invalid,
    Unimplemented,
    [NameIn("text")]
    Text,
    [NameIn("at")]
    At,
    [NameIn("image")]
    Image,
    [NameIn("record")]
    Record,
    [NameIn("reply")]
    Reply,
    [NameIn("face")]
    Face,
}