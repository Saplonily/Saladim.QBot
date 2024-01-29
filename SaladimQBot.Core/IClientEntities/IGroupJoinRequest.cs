namespace SaladimQBot.Core;

/// <summary>
/// 加群请求
/// </summary>
public interface IGroupJoinRequest : IClientEntity
{
    /// <summary>
    /// bot号所在的群
    /// </summary>
    IJoinedGroup Group { get; }

    /// <summary>
    /// 请求的用户
    /// </summary>
    IUser User { get; }

    /// <summary>
    /// 验证信息
    /// </summary>
    string Comment { get; }

    /// <summary>
    /// 允许
    /// </summary>
    /// <returns>该群用户</returns>
    Task<IGroupUser> ApproveAsync();

    /// <summary>
    /// 拒绝
    /// </summary>
    /// <param name="reason">原因</param>
    Task DisapproveAsync(string? reason);
}
