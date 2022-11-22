namespace SaladimQBot.Core;

/// <summary>
/// 一个群里的QQ用户
/// </summary>
public interface IGroupUser : IUser
{
    /// <summary>
    /// 该用户所在的群
    /// </summary>
    IJoinedGroup Group { get; }

    /// <summary>
    /// 群名片
    /// </summary>
    string Card { get; }

    /// <summary>
    /// 地区
    /// </summary>
    string Area { get; }

    /// <summary>
    /// 加群时间
    /// </summary>
    DateTime JoinTime { get; }

    /// <summary>
    /// 最后发言的时间
    /// </summary>
    DateTime LastMessageSentTime { get; }

    /// <summary>
    /// 用户的群等级
    /// </summary>
    string GroupLevel { get; }

    /// <summary>
    /// 用户在群里的身份
    /// </summary>
    GroupRole GroupRole { get; }

    /// <summary>
    /// 用户是否是不良记录成员
    /// </summary>
    bool IsUnFriendly { get; }

    /// <summary>
    /// 用户群头衔
    /// </summary>
    string GroupTitle { get; }

    /// <summary>
    /// 用户群头衔过期时间
    /// </summary>
    DateTime GroupTitleExpireTime { get; }

    /// <summary>
    /// 是否允许修改群名片
    /// </summary>
    bool IsAbleToChangeCard { get; }

    /// <summary>
    /// 禁言到期时间
    /// </summary>
    DateTime MuteExpireTime { get; }
}
