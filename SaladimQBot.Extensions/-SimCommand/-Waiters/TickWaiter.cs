using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

/// <summary>
/// tick等待器
/// </summary>
public class TickWaiter : EventWaiter
{
    protected int ticks = 0;

    public override EventWaiterChecker Checker { get; }

    /// <summary>
    /// 使用ticks(秒数)初始化一个waiter
    /// </summary>
    /// <param name="ticks">秒数, 在每次被push事件时减一秒</param>
    public TickWaiter(int ticks)
    {
        this.ticks = ticks;
        Checker = TryTickAndCheck;
    }

    protected bool TryTickAndCheck(IClientEvent e)
        => e is IClientTickEvent && --ticks <= 0;
}
