using System.Collections;
using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public class CoroutineService
{
    protected List<IEnumerator<EventWaiter>> coroutines;

    public CoroutineService()
    {
        coroutines = new();
    }

    /// <summary>
    /// 增加一个新的事件处理协程, 会被立即<see cref="IEnumerator.MoveNext()"/>一次
    /// </summary>
    /// <param name="enumerator"></param>
    public void AddNewCoroutine(IEnumerator<EventWaiter> enumerator)
    {
        if (enumerator.MoveNext())
        {
            lock (this)
            {
                coroutines.Add(enumerator);
            }
        }
    }

    /// <summary>
    /// 移除一个协程
    /// </summary>
    /// <param name="enumerator"></param>
    public void RemoveCoroutine(IEnumerator<EventWaiter> enumerator)
    {
        coroutines.Remove(enumerator);
    }

    /// <summary>
    /// 使用一个事件推动该协程
    /// 在遇到EventWaiter时会停止并且等待该事件
    /// </summary>
    /// <param name="clientEvent"></param>
    public void PushCoroutines(IIClientEvent clientEvent)
    {
        lock (this)
        {
            List<IEnumerator<EventWaiter>> markedRemoves = new();
            foreach (var ie in coroutines)
            {
                if (ie.Current.Checker(clientEvent))
                {
                    if (ie.MoveNext())
                        continue;
                    else
                        markedRemoves.Add(ie);
                }
            }
            for (int i = coroutines.Count - 1; i >= 0; i--)
            {
                if (markedRemoves.Contains(coroutines[i]))
                {
                    markedRemoves.Remove(coroutines[i]);
                    coroutines.RemoveAt(i);
                }
            }
        }
    }

    public bool IsRunning(IEnumerator<EventWaiter> coroutine)
    {
        return coroutines.Contains(coroutine);
    }
}