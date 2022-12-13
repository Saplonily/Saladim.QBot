namespace SaladimQBot.Core;

/// <summary>
/// 添加好友请求
/// </summary>
public interface IFriendAddRequest : IClientEntity
{
    /// <summary>
    /// 请求的用户
    /// </summary>
    IUser User { get; }

    /// <summary>
    /// 附加的验证信息
    /// </summary>
    string Comment { get; }

    /// <summary>
    /// 接受加好友请求
    /// </summary>
    /// <returns>加上好友后的<see cref="IFriendUser"/>实体</returns>
    Task<IFriendUser> ApproveAsync();

    /// <summary>
    /// 拒绝加好友请求
    /// </summary>
    Task DisapproveAsync();
}
