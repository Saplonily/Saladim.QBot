using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public class MessageWaiter : EventWaiter
{
    public override EventWaiterChecker Checker { get; }

    public MessageWaiter(Func<IMessage, bool> checker, Action<IMessage> reporter)
    {
        Checker = e =>
        {
            if (e is IClientMessageReceivedEvent receivedEvent)
            {
                reporter(receivedEvent.Message);
                return checker(receivedEvent.Message);
            }
            else
            {
                return false;
            }
        };
    }

    public MessageWaiter(IUser user, Action<IMessage> reporter)
    {
        Checker = e =>
        {
            if (e is IClientMessageReceivedEvent receivedEvent)
            {
                if (receivedEvent.Message.Sender.IsSameUser(user))
                {
                    reporter(receivedEvent.Message);
                    return true;
                }
            }
            return false;
        };
    }

    public MessageWaiter(IGroupUser groupUser, Action<IGroupMessage> reporter)
    {
        Checker = e =>
        {
            if (e is IClientGroupMessageReceivedEvent receivedEvent)
            {
                if (receivedEvent.Message.Sender.IsSameGroupUser(groupUser))
                {
                    reporter(receivedEvent.Message);
                    return true;
                }
            }
            return false;
        };
    }
}
