using QBotDotnet.GoCqHttp;
using QBotDotnet.GoCqHttp.Internal;

namespace QBotDotnet.Public;

public class MessageBuilder
{
    protected List<CQMessagePart> parts = new();

    public MessageBuilder AddText(string text)
    {
        parts.Add(new CQTextMessagePart(text));
        return this;
    }

    public CQMessage Build()
    {
        CQMessage msg = new();
        msg.CQParts.AddRange(parts);
        return msg;
    }
}