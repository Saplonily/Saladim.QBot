using System.Diagnostics;
using QBotDotnet.GoCqHttp;
using QBotDotnet.GoCqHttp.Internal;

namespace QBotDotnet.Public;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Message
{
    internal Client Client;
    internal CQMessagePost RawPost { get; private set; }
    public CQMessage Content { get => RawPost.Message; }
    public string RawContent { get => RawPost.RawMessage; }
    public DateTime SendTime { get => RawPost.DateTime; }
    public User Sender { get; protected set; }

    internal Message(CQMessagePost rawPost, Client client)
    {
        RawPost = rawPost;
        Client = client;
        Sender = User.GetFromAsync(rawPost.Sender, client);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public string DebuggerDisplay
    {
        get => $"{Sender.Name}: {RawContent}";
    }
}