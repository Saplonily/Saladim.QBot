namespace SaladimQBot.Core;

/// <summary>
/// <para>消息节点类型的枚举</para>
/// <para>暂未被支持完全,未支持的节点类型为<see cref="Unimplemented"/></para>
/// </summary>
public enum MessageNodeType
{
    Invalid,
    Unimplemented,
    Text,
    At,
    Image,
    Record,
    Reply,
    Face
}