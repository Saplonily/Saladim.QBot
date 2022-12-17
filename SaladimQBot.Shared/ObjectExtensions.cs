using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SaladimQBot.Shared;

public static class ObjectExtensions
{
    /// <summary>
    /// 强转
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cast<T>(this object obj) where T : notnull
        => (T)obj;

    /// <summary>
    /// 强转,转换失败时返回<see langword="null"/>,仅引用类型有效. 与 <see langword="as"/> 类似
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// 异步获取<see cref="Lazy{T}.Value"/>
    /// </summary>
    /// <typeparam name="T">Lazy泛型类型</typeparam>
    /// <param name="lazy">lazy实例</param>
    /// <returns>lazy的值(异步)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<T> GetValueAsync<T>(this Lazy<T> lazy)
        => Task.Factory.StartNew(() => lazy.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetResultOfAwaiter<T>(this Task<T> task)
        => task.GetAwaiter().GetResult();
}