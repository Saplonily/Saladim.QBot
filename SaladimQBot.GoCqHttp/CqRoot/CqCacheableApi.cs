namespace SaladimQBot.GoCqHttp;

public abstract class CqCacheableApi : CqApi
{
    [Ignore]
    public bool UseCache { get; set; } = true;

    [Name("no_cache")]
    public bool NoCache { get => !UseCache; set => UseCache = !value; }

    public override bool Equals(object? obj)
    {
        return obj is CqCacheableApi api &&
               ApiName == api.ApiName &&
               UseCache == api.UseCache;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ApiName, UseCache);
    }
}