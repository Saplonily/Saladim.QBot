using QBotDotnet.GoCqHttp.Internal;

namespace QBotDotnet.Public;

public class PrivateMessage : Message
{
    public PrivateMessageTempSource TempSource { get; private set; }
    public PrivateMessage(CQPrivateMessagePost rawPost, Client client) : base(rawPost, client)
    {
        TempSource = rawPost.TempSource;
    }
}