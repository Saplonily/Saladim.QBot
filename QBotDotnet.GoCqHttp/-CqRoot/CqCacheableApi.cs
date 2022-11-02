namespace QBotDotnet.GoCqHttp;

public abstract class CqCacheableApi : CqApi
{
    [Ignore]
    public virtual bool UseCache { get; set; }

    [Name("no_cache")]
    public bool NoCache { get => !UseCache; set => UseCache = !value; }
}