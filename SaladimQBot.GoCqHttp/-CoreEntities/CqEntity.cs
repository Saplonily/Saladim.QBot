using System.Diagnostics;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public abstract class CqEntity : IClientEntity
{
    public CqClient Client { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IClient IClientEntity.Client { get => Client; }

    public CqEntity(CqClient client)
    {
        Client = client;
    }
}
