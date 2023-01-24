namespace SaladimQBot.Extensions;

public class OrEventWaiter : EventWaiter
{
    public override EventWaiterChecker Checker { get; }

    public OrEventWaiter(EventWaiter waiterA, EventWaiter waiterB, Action<EventWaiter> reporter)
    {
        Checker = e =>
        {
            //虽然但是我还是奇奇怪怪的实现了短路求值
            if (waiterA.Checker(e))
            {
                reporter(waiterA);
                return true;
            }
            else if (waiterB.Checker(e))
            {
                reporter(waiterB);
                return true;
            }
            return false;
        };
    }
}
