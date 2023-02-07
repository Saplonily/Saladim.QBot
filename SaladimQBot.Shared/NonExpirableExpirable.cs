using System.Diagnostics;

namespace SaladimQBot.Shared;

[DebuggerDisplay("[NonExpirable] {Value}")]
public class NonExpirableExpirable<T> : IExpirable<T>
{
    protected Lazy<T> lazyValue;

    public T Value => lazyValue.Value;

    public bool IsExpired => false;

    public NonExpirableExpirable(T initValue)
    {
#if NETSTANDARD2_0
        lazyValue = new(() => initValue, true);
#else
        lazyValue = new(initValue);
#endif
    }

    public NonExpirableExpirable(Func<T> valueFactory)
    {
        lazyValue = new(valueFactory, true);
    }
}
