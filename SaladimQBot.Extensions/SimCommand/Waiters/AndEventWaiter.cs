namespace SaladimQBot.Extensions;

public class AndEventWaiter : EventWaiter
{
    public override EventWaiterChecker Checker { get; }

    public AndEventWaiter(EventWaiter waiterA, EventWaiter waiterB)
    {
        Checker = e => waiterA.Checker(e) && waiterB.Checker(e);
    }
}
