using System.Diagnostics;

namespace SaladimQBot.Shared;

public static class EnumHelper
{
    [DebuggerStepThrough]
    public static TEnum ToObject<TEnum>(int value) where TEnum : Enum
    {
        return (TEnum)Enum.ToObject(typeof(TEnum), value);
    }

    [DebuggerStepThrough]
    public static TEnum ToObject<TEnum, TValue>(TValue value) where TValue : struct where TEnum : Enum
    {
        return (TEnum)Enum.ToObject(typeof(TEnum), value);
    }
}