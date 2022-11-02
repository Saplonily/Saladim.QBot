﻿using System.Diagnostics;

namespace QBotDotnet.SharedImplement;

public static class ExceptionHelper
{
    [DebuggerStepThrough]
    public static IEnumerable<Exception> GetChainedExceptions(Exception exception)
    {
        List<Exception> list = new();
        Exception? cur = exception;
        while (cur != null)
        {
            list.Add(cur);
            cur = cur.InnerException;
        }
        return list;
    }
}