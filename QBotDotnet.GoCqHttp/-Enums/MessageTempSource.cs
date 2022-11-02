using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp;

/// <summary>
/// <para>
/// <see cref="MessageTempSource"/>与<see cref="Core.MessageTempSource"/>
/// 互相紧密关联,区别仅仅为是否加入特性装饰.
/// <para>
/// 如果需要更改其一请注意同时更改两处
/// 以保证枚举间基于底层值的强转不会出现问题
/// </para>
/// </para>
/// </summary>
[JsonConverter(typeof(CqEnumJsonConverter))]
public enum MessageTempSource
{
    Invalid,
    [NameIn(0)]
    Group,
    [NameIn(1)]
    QQConsult,
    [NameIn(2)]
    Search,
    [NameIn(3)]
    QQFilm,
    [NameIn(4)]
    HotChat,
    [NameIn(6)]
    Verification,
    [NameIn(7)]
    MultiPersonChat,
    [NameIn(8)]
    Date,
    [NameIn(9)]
    AddressBook
}