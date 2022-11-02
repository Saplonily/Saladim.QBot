using System.Diagnostics;

namespace QBotDotnet.SharedImplement;

public static class ObjectExtensions
{
    /// <summary>
    /// 强转
    /// </summary>
    [DebuggerStepThrough]
    public static T Cast<T>(this object obj) where T : notnull
        => (T)obj;

    /// <summary>
    /// 强转,转换失败时返回null,仅引用类型有效. 与 <see langword="as"/> 类似
    /// </summary>
    [DebuggerStepThrough]
    public static T? AsCast<T>(this object? obj) where T : class
    {
        if (obj is null) return null;
        try
        {
            return obj.Cast<T>();
        }
        catch (InvalidCastException)
        {
            return null;
        }
    }
}