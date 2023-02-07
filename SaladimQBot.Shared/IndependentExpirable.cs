using System.Diagnostics;

namespace SaladimQBot.Shared;

[DebuggerDisplay("{(!hasValueCache ? \"[NoValueCached]\" : \"\")} {valueCache}")]
public class IndependentExpirable<T> : IIndependentExpirable<T>, IExpirable<T>
{
    protected Func<T> valueFactory;
    protected T? valueCache = default;
    protected bool hasValueCache = false;
    protected TimeSpan expireTimeSpan;
    protected DateTime expireDateTime;

    public TimeSpan ExpireTimeSpan => expireTimeSpan;

    public bool IsExpired => !hasValueCache || DateTime.Now > expireDateTime;

    public T Value
    {
        get
        {
            if (!IsExpired)
                return valueCache!;
            lock (valueFactory)
            {
                if (!IsExpired)
                    return valueCache!;
                T newValue = valueFactory();
                expireDateTime = DateTime.Now + expireTimeSpan;
                valueCache = newValue;
                hasValueCache = true;
                return newValue;
            }
        }
    }

    public IndependentExpirable(Func<T> valueFactory, TimeSpan expireTimeSpan)
    {
        this.valueFactory = valueFactory;
        this.expireTimeSpan = expireTimeSpan;
    }

    public IndependentExpirable(T presetValue, Func<T> valueFactory, TimeSpan expireTimeSpan)
    {
        this.expireDateTime = DateTime.Now + expireTimeSpan;
        this.valueCache = presetValue;
        this.valueFactory = valueFactory;
        this.expireTimeSpan = expireTimeSpan;
    }
}

public interface IIndependentExpirable<out T> : IExpirable<T>
{

}