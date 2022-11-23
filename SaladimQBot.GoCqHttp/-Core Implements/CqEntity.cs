using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public abstract class CqEntity : IClientEntity
{
    public ICqClient Client { get; }

    IClient IClientEntity.Client { get => Client; }

    public CqEntity(ICqClient client)
    {
        Client = client;
    }
}
