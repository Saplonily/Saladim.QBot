namespace SaladimQBot.Extensions;

/// <summary>
/// 事件等待器的or逻辑门包装 (实现是短路求值的)
/// </summary>
public class OrEventWaiter : EventWaiter
{
    /// <summary>
    /// 内部Checker
    /// </summary>
    public override EventWaiterChecker Checker { get; }

    /// <summary>
    /// 使用两个waiter和一个报告器初始化
    /// </summary>
    /// <param name="reporter">报告器</param>
    /// <param name="waiterA">waiter1</param>
    /// <param name="waiterB">waiter2</param>
    public OrEventWaiter(Action<EventWaiter> reporter, EventWaiter waiterA, EventWaiter waiterB)
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

    /// <summary>
    /// 使用多个waiter和一个报告器初始化
    /// </summary>
    /// <param name="reporter">报告器</param>
    /// <param name="waiterA">waiter1</param>
    /// <param name="waiterB">waiter2</param>
    /// <param name="waiters">更多waiter</param>
    public OrEventWaiter(Action<EventWaiter> reporter, EventWaiter waiterA, EventWaiter waiterB, params EventWaiter[] waiters)
    {
        Checker = e =>
        {
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
            else
            {
                foreach (var waiter in waiters)
                {
                    bool flag = waiter.Checker(e);
                    if (flag)
                    {
                        reporter(waiter);
                        return true;
                    }
                }
            }
            return false;
        };
    }
}
