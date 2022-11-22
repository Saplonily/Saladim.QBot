namespace SaladimQBot.Core;

/// <summary>
/// 一个QQ用户
/// </summary>
public interface IUser
{
    /// <summary>
    /// QQ号
    /// </summary>
    long UserId { get; }

    /// <summary>
    /// 昵称
    /// </summary>
    string Nickname { get; }

    /// <summary>
    /// 性别
    /// </summary>
    Sex Sex { get; }

    /// <summary>
    /// 年龄
    /// </summary>
    int Age { get; }

    /// <summary>
    /// Qid(一个类似QQ号的东西)
    /// </summary>
    string Qid { get; }

    /// <summary>
    /// 等级
    /// </summary>
    int Level { get; }

    /// <summary>
    /// 连续登录天数
    /// </summary>
    int LoginDays { get; }

    Task<IPrivateMessage> SendPrivateMessage(IMessageEntity messageEntity);
}
