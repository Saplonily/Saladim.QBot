using System.Diagnostics;
using System.Text.Json;
using QBotDotnet.Public;

namespace QBotDotnet.GoCqHttp.Internal;

[DebuggerDisplay("{NickName}({UserId})")]
public class CQMessageSender
{
    public long UserId { get; private set; } = -1;
    public string NickName { get; private set; } = string.Empty;
    public Sex Sex { get; private set; } = Sex.Unknown;
    public int Age { get; private set; } = -1;
    protected CQMessageSender() { }

    internal static CQMessageSender GetFrom(JsonElement je, bool doUpdate)
    {
        CQMessageSender sender = new();
        sender.LoadFrom(je);
        if (doUpdate && je.GetProperty("sender").TryGetProperty("level", out _))
        {
            CQGroupMessageSender updatedSender = CQGroupMessageSender.GetFrom(je);
            return updatedSender;
        }
        return sender;
    }

    internal virtual void LoadFrom(JsonElement rootJE)
    {
        try
        {
            var je = rootJE.GetProperty("sender");
            UserId = je.GetProperty("user_id").GetInt64();
            NickName = je.GetProperty("nickname").GetString() ?? string.Empty;
            Sex = QTypeHelper.ParseSex(je.GetProperty("sex").GetString() ?? string.Empty);
            Age = je.GetProperty("age").GetInt32();

        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(rootJE), e);
        }
    }
}
