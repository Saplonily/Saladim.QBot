using SaladimQBot.Core;

namespace SaladimQBot.Konata;

public class KqEntity : IClientEntity
{
    public KqClient Client { get; }

    IClient IClientEntity.Client => Client;

    public KqEntity(KqClient client)
    {
        Client = client;
    }
}
