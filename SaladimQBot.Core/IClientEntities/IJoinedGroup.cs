namespace SaladimQBot.Core;

/// <summary>
/// 一个bot号加入的QQ群
/// </summary>
public interface IJoinedGroup : IGroup, IMessageWindow
{
    /// <summary>
    /// 群创建时间
    /// </summary>
    DateTime CreateTime { get; }

    /// <summary>
    /// 群等级
    /// </summary>
    uint GroupLevel { get; }

    /// <summary>
    /// 成员个数
    /// </summary>
    int MembersCount { get; }

    /// <summary>
    /// 最大成员个人 / 群容量
    /// </summary>
    int MaxMembersCount { get; }

    /// <summary>
    /// 成员
    /// </summary>
    IEnumerable<IGroupUser> Members { get; }
}
