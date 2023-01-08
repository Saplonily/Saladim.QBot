using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public class CommandWaiter : EventWaiter
{
    protected bool curState = false;

    protected readonly Func<MethodBasedCommand, CommandContent, object[], bool> checker;

    public override EventWaiterChecker Checker { get; }

    public CommandWaiter(SimCommandExecuter simCommandExecuter, Func<MethodBasedCommand, CommandContent, object[], bool> checker)
    {
        simCommandExecuter.OnCommandExecuted += this.SimCommandExecutor_OnCommandExecuted;
        this.checker = checker;
        Checker = DefaultChecker;
    }

    public CommandWaiter(SimCommandExecuter simCommandExecuter, IUser executor, string cmdName, object[] args)
    {
        simCommandExecuter.OnCommandExecuted += this.SimCommandExecutor_OnCommandExecuted;
        this.checker = (cmd, content, checkerArgs) =>
        {
            bool sameCmdSame = cmd.Name == cmdName;
            bool sameUser = content.Executor.IsSameUser(executor);
            if (!sameCmdSame || !sameUser) return false;
            if (!cmd.IsParamsCommand)
                return checkerArgs.SequenceEqual(args);
            else
                return Enumerable.SequenceEqual(((Array)args[0]).Cast<object>(), ((Array)checkerArgs[0]).Cast<object>());
        };
        Checker = DefaultChecker;
    }

    public CommandWaiter(SimCommandExecuter simCommandExecuter, IUser executor, string cmdName)
        : this(simCommandExecuter, executor, cmdName, Array.Empty<object>())
    {

    }

    public CommandWaiter(SimCommandExecuter simCommandExecuter, IUser executor, string cmdName, Action<object[]> argsReporter)
    {
        simCommandExecuter.OnCommandExecuted += this.SimCommandExecutor_OnCommandExecuted;
        this.checker = (cmd, content, checkerArgs) =>
        {
            if (content.Executor.IsSameUser(executor) && cmd.Name == cmdName)
            {
                argsReporter(checkerArgs);
                return true;
            }
            return false;
        };
        Checker = DefaultChecker;
    }

    public CommandWaiter(SimCommandExecuter simCommandExecuter, IGroupUser executor, string cmdName, Action<object[]> argsReporter)
    {
        simCommandExecuter.OnCommandExecuted += this.SimCommandExecutor_OnCommandExecuted;
        this.checker = (cmd, content, checkerArgs) =>
        {
            if (content.Executor is IGroupUser groupUser && groupUser.IsSameGroupUser(executor) && cmd.Name == cmdName)
            {
                argsReporter(checkerArgs);
                return true;
            }
            return false;
        };
        Checker = DefaultChecker;
    }

    protected bool DefaultChecker(IIClientEvent clientEvent)
    {
        if (curState)
        {
            curState = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    protected void SimCommandExecutor_OnCommandExecuted(MethodBasedCommand command, CommandContent content, object[] @params)
    {
        if (checker(command, content, @params))
        {
            curState = true;
        }
    }
}
