﻿namespace SaladimQBot.Core;

/// <summary>
/// 消息链中的 @ 节点
/// </summary>
public interface IMessageChainAtNode : IMessageChainNode
{
    /// <summary>
    /// 被@的用户, 在类型为@全体成员时为空
    /// </summary>
    IUser? User { get; }

    /// <summary>
    /// 在发送时若不在群时的显示昵称
    /// 作发送节点时应实现为<see langword="null"/>
    /// </summary>
    string? UserName { get; }
}