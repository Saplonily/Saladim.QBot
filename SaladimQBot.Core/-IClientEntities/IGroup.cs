namespace SaladimQBot.Core;

/// <summary>
/// 一个群,允许bot号不在群内
/// </summary>
public interface IGroup : IClientEntity
{
    /// <summary>
    /// 群号
    /// </summary>
    long GroupId { get; }

    /// <summary>
    /// 群名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 群备注
    /// </summary>
    string Remark { get; }

    /// <summary>
    /// 群头像url
    /// </summary>
    string AvatarUrl { get; }
}
