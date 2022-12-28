namespace SaladimQBot.Shared;

public interface IExpirable<out T>
{
    T Value { get; }

    bool IsExpired { get; }
}

public static class IExpirableExtensions
{
    public static Task<T> GetValueAsync<T>(this IExpirable<T> expirable)
        => expirable.IsExpired ? Task.Run(() => expirable.Value) : Task.FromResult(expirable.Value);
}