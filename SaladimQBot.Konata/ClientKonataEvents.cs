using SaladimQBot.Core;

namespace SaladimQBot.Konata;

public class ClientEvent : IClientEvent
{
    public KqClient SourceClient { get; }

    IClient IClientEvent.SourceClient => SourceClient;

    public ClientEvent(KqClient kqClient)
    {
        SourceClient = kqClient;
    }
}

public class ClientGroupMessageReceivedEvent : ClientEvent, IClientGroupMessageReceivedEvent
{
    public IGroupMessage Message { get; }

    public IGroupUser Sender { get; }

    public IJoinedGroup Group { get; }

    IMessage IClientMessageReceivedEvent.Message => Message;

    public ClientGroupMessageReceivedEvent(KqClient kqClient,
        IGroupMessage message, 
        IGroupUser sender,
        IJoinedGroup group) : base(kqClient)
    {
        this.Message = message;
        this.Sender = sender;
        this.Group = group;
    }
}
