namespace SaladimQBot.GoCqHttp;

public abstract class CqEntity
{
    public ICqClient Client { get; }

    public CqEntity(ICqClient client)
    {
        Client = client;
    }
}
