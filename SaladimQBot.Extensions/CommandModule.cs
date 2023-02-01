using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

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
    public SimCommandExecuter SimCommandExecuter { get; }

    public IMessage Message { get; }

    public IUser Executor => Message.Sender;

    public IMessageWindow MessageWindow => Message.MessageWindow;

    public IClient Client => Message.Client;

    public bool ExecutedInGroup => Message is IGroupMessage;

    public long ExecutorId => Executor.UserId;

    public CommandContent(SimCommandExecuter service, IMessage message)
    {
        this.SimCommandExecuter = service;
        this.Message = message;
    }
}