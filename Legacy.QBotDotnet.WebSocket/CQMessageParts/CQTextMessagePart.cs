namespace QBotDotnet.GoCqHttp.Internal;

public class CQTextMessagePart : CQMessagePart
{
    public CQTextMessagePart(string text) : base(
            CQPartType.Text, new Dictionary<string, string>()
            {
                ["text"] = text
            }
        )
    { }
}