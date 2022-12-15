﻿using SaladimQBot.Core;

namespace SaladimQBot.SimCommand;

/// <summary>
/// 指令模块的基类
/// </summary>
public abstract class CommandModule
{
    /// <summary>
    /// 关于这个指令的信息
    /// </summary>
    public CommandContent Content { get; internal set; } = default!;

    /// <summary>
    /// <para>预检查, 帮助预先快速过滤不需要的Content</para>
    /// <para>在GoCqHttp中建议排除临时会话消息</para>
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public virtual bool PreCheck(CommandContent content) { return true; }
}

public sealed class CommandContent
{
    public SimCommandService Service { get; }

    public IMessage Message { get; }

    public IUser Executer => Message.Sender;

    public IMessageWindow MessageWindow => Message.MessageWindow;

    public IClient Client => Message.Client;

    public CommandContent(SimCommandService service, IMessage message)
    {
        this.Service = service;
        this.Message = message;
    }
}