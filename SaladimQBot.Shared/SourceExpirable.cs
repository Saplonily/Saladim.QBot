using System.Diagnostics;

namespace SaladimQBot.Shared;

[DebuggerDisplay("IndepenExp -> {independentExpirable}")]
public class SourceExpirable<T> : IDependencyExpirable<T>
{
    protected IndependentExpirable<T> independentExpirable;

    public IDependencyExpirable<object> Upstream => (IDependencyExpirable<object>)this;

    public T Value => independentExpirable.Value;

    public bool IsExpired => independentExpirable.IsExpired;

    public SourceExpirable(IndependentExpirable<T> independentExpirable)
    {
        this.independentExpirable = independentExpirable;
    }
}
