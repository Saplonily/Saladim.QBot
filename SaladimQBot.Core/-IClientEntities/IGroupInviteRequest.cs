namespace SaladimQBot.Core;

public interface IGroupInviteRequest
{
    /// <summary>
    /// 被邀请入的群
    /// </summary>
    IGroup Group { get; }

    /// <summary>
    /// 发起邀请的用户
    /// </summary>
    IUser User { get; }

    /// <summary>
    /// 允许
    /// </summary>
    /// <returns></returns>
    Task ApproveAsync();

    /// <summary>
    /// 拒绝
    /// </summary>
    Task DisapproveAsync();
}
