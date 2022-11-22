namespace SaladimQBot.GoCqHttp;

public interface IExpirableValueGetter
{
    internal Expirable<TResultData> MakeExpirableApiCallResultData<TResultData>(CqApi api)
        where TResultData : CqApiCallResultData;

    internal Expirable<TChild> MakeDependencyExpirable<TChild, TFather>(
        Expirable<TFather> dependentFather,
        Func<TFather, TChild> valueGetter
        ) where TChild : notnull where TFather : notnull;

    internal Expirable<TChild> MakeDependencyExpirable<TChild, TFather>(
        Expirable<TFather> dependentFather,
        TChild presetValue,
        Func<TFather, TChild> valueGetter
        ) where TChild : notnull where TFather : notnull;
    Expirable<T> MakeNoneExpirableExpirable<T>(Func<T> valueFactory) where T : notnull;
}

internal static class IExpirableValueGetterExtensions
{
    internal static Expirable<T> MakeNoneExpirableExpirable<T>(this IExpirableValueGetter getter, T value) where T : notnull
    {
        return getter.MakeNoneExpirableExpirable(() => value);
    }
}