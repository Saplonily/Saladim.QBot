using System.Diagnostics;

namespace SaladimQBot.Shared;

[DebuggerDisplay("UpStream -> {upstream}")]
public class DependencyExpirable<T> : IDependencyExpirable<T>, IExpirable<T>
{
    protected IndependentExpirable<T>? presetIndependent;
    protected Func<object, T> valueGetter;
    protected IDependencyExpirable<object> upstream;

    public T Value => presetIndependent?.IsExpired is (true or null) ? valueGetter(upstream.Value) : presetIndependent.Value;

    public bool IsExpired => presetIndependent?.IsExpired is (true or null) ? upstream.IsExpired : presetIndependent.IsExpired;

    private DependencyExpirable(Func<object, T> valueGetter, IDependencyExpirable<object> upstream)
    {
        this.valueGetter = valueGetter;
        this.upstream = upstream;
    }

    public static DependencyExpirable<T> Create<TSource>(IDependencyExpirable<TSource> source, Func<TSource, T> valueGetter)
        where TSource : class
    {
        return new DependencyExpirable<T>(obj => valueGetter((TSource)obj), source);
    }

    public static DependencyExpirable<T> Create<TSource>(
        T presetValue,
        TimeSpan presetValueExpireTimeSpan,
        IDependencyExpirable<TSource> source,
        Func<TSource, T> valueGetter
        )
        where TSource : class
    {
        var s = Create(source, valueGetter);
        s.presetIndependent = new(presetValue, () => presetValue, presetValueExpireTimeSpan);
        return s;
    }

    public IDependencyExpirable<object> Upstream => upstream;
}

public interface IDependencyExpirable<out T> : IExpirable<T>
{
    IDependencyExpirable<object> Upstream { get; }
}