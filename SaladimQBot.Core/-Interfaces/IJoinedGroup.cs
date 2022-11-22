namespace SaladimQBot.Core;

/// <summary>
/// 一个bot号加入的QQ群
/// </summary>
public interface IJoinedGroup : IGroup
{
    #region 属性
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
    #endregion

    #region 方法

    /// <summary>
    /// 异步向群里发送一条消息, 使用消息实体发送
    /// </summary>
    /// <param name="messageEntity">消息实体</param>
    /// <returns>一个IGroupMessage实例</returns>
    Task<IGroupMessage> SendMessageAsync(IMessageEntity messageEntity);

    /// <summary>
    /// 异步向群里发送一条消息, 使用格式化字符串发送
    /// </summary>
    /// <param name="message">字符串</param>
    /// <returns>一个IGroupMessage实例</returns>
    Task<IGroupMessage> SendMessageAsync(string message);

    #endregion
}
