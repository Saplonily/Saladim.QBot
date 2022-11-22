using System.Diagnostics;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

//TODO 重构Expirable

/// <summary>
/// 会过期的值类型
/// </summary>
[DebuggerDisplay("{DebuggerDisplayPrefix,nq}{valueCache,nq}")]
public class Expirable<T> where T : notnull
{
    protected readonly Func<T> newValueFactory;
    //2022-11-12 11:27:09 注:当T是struct时valueCache仍是T类型而非Nullable<T>
    protected T? valueCache;
    protected bool hasValueCache = false;

    /// <summary>
    /// 过期时间, 为null表示已被强制过期
    /// </summary>
    public DateTime? ExpireTime { get; private set; }

    /// <summary>
    /// <para>过期时间跨度, 在值过期重获之后过期时间为<see cref="DateTime.Now"/>+<see cref="TimeSpanExpired"/></para>
    /// <para>当被指定为负数时表示过期重获后不会再过期(类似于Lazy)</para>
    /// </summary>
    public TimeSpan TimeSpanExpired { get; internal set; }

    /// <summary>
    /// 是否有值缓存,有值缓存情况下且没过期时Value立即返回
    /// </summary>
    public bool HasValueCache { get => hasValueCache; }

    /// <summary>
    /// 是否已过期(被强制过期/时间超过预设时间/没有值缓存 三者均满足才返回false)
    /// </summary>
    public bool IsExpired { get => ExpireTime is null || DateTime.Now > ExpireTime || hasValueCache is false; }

    public T Value
    {
        get
        {
            if (this.IsExpired)
            {
                lock (this)
                {
                    if (this.IsExpired)
                    {
                        var value = newValueFactory();
                        valueCache = value;
                        hasValueCache = true;
                        ExpireTime =
                            TimeSpanExpired >= TimeSpan.Zero ?
                            DateTime.Now + TimeSpanExpired :
                            DateTime.MaxValue;
                        return valueCache;
                    }
                    return valueCache!;
                }
            }
            return valueCache!;
        }
    }

    public Task<T> ValueAsync
    {
        get
        {
            if (IsExpired)
            {
                return Task.Run(() => Value);
            }
            return Task.FromResult(Value);
        }
    }

    protected Expirable(Func<T> newValueFactory, DateTime? timeExpired)
    {
        this.newValueFactory = newValueFactory;
        ExpireTime = timeExpired;
    }

    protected Expirable(T presetValue, Func<T> newValueFactory, DateTime? timeExpired)
    {
        this.newValueFactory = newValueFactory;
        ExpireTime = timeExpired;
        valueCache = presetValue;
        hasValueCache = true;
    }

    internal Expirable(Func<T> newValueFactory, DateTime? timeExpired, TimeSpan timeSpanToExpired)
    {
        this.newValueFactory = newValueFactory;
        ExpireTime = timeExpired;
        TimeSpanExpired = timeSpanToExpired;
    }

    internal Expirable(Func<T> newValueFactory, T presetValue, DateTime? timeExpired, TimeSpan timeSpanToExpired)
    {
        this.newValueFactory = newValueFactory;
        ExpireTime = timeExpired;
        valueCache = presetValue;
        hasValueCache = true;
        TimeSpanExpired = timeSpanToExpired;
    }

    internal Expirable(Func<T> newValueFactory, TimeSpan timeSpanToExpired)
        : this(newValueFactory, DateTime.Now + timeSpanToExpired)
    {
        TimeSpanExpired = timeSpanToExpired;
    }

    internal Expirable(Func<T> newValueFactory, T presetValue, TimeSpan timeSpanToExpired)
        : this(presetValue, newValueFactory, DateTime.Now + timeSpanToExpired)
    {
        TimeSpanExpired = timeSpanToExpired;
    }

    /// <summary>
    /// 强制使值失效
    /// </summary>
    public void MakeExpire()
    {
        ExpireTime = null;
    }

    public override string ToString()
    {
        if (!hasValueCache)
            return "[NoValueCache]";
        if (IsExpired)
            return $"[Expired] {valueCache}";
        return $"{valueCache}";
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal string DebuggerDisplayPrefix
    {
        get
        {
            if (!hasValueCache)
                return "[NoValueCache] ";
            if (IsExpired)
                return $"[Expired] ";
            return "";
        }
    }
}

public class CastedExpirable<T, TSource> : Expirable<T> where T : notnull where TSource : notnull
{
    private CastedExpirable(Func<T> newValueFactory, DateTime? timeExpired, TimeSpan timeSpanToExpired)
        : base(newValueFactory, timeExpired, timeSpanToExpired)
    {
    }

    public Expirable<TSource> CastSource { get; protected set; } = null!;

    public static CastedExpirable<T, TSource> MakeFromSource(Expirable<TSource> source)
    {
        return new CastedExpirable<T, TSource>(() => source.Value.Cast<T>(), source.ExpireTime, source.TimeSpanExpired)
        {
            CastSource = source
        };
    }
}