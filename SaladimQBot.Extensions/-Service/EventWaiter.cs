using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public abstract class EventWaiter
{
    public abstract EventWaiterChecker Checker { get; }
}

public delegate bool EventWaiterChecker(IClientEvent clientEvent);