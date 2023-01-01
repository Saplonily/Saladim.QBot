using System.Collections;
using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public class CoroutineService
{
    protected List<IEnumerator> coroutines;

    public CoroutineService()
    {
        coroutines = new();
    }

    public void StartNewCoroutine(IEnumerator enumerator)
    {
        coroutines.Add(enumerator);
    }


}

public struct MessageWaiter
{
    public MessageWaiterChecker Checker;

    public MessageWaiter(MessageWaiterChecker checker)
    {
        Checker = checker;
    }
}

public delegate bool MessageWaiterChecker(IMessage message);