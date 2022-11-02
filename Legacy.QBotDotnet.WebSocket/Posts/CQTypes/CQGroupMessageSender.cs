using System.Text.Json;
using QBotDotnet.Public;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGroupMessageSender : CQMessageSender
{
    public string Card { get; private set; } = string.Empty;
    public string Area { get; private set; } = string.Empty;
    public string Level { get; private set; } = string.Empty;
    public GroupRole Role { get; private set; } = GroupRole.Invalid;
    public string Title { get; private set; } = string.Empty;
    public long GroupId { get; private set; } = -1;
    protected CQGroupMessageSender() { }

    internal override void LoadFrom(JsonElement rootJE)
    {
        base.LoadFrom(rootJE);
        try
        {
            var je = rootJE.GetProperty("sender");
            Area = je.GetProperty("area").GetString() ?? string.Empty;
            Level = je.GetProperty("level").GetString() ?? string.Empty;
            //匿名消息没有Role和Title,但是这里我们隐式指定为Member和Anonymous
            Role = GroupRole.Member;
            if (je.TryGetProperty("role", out JsonElement roleEle))
            {
                Role = QTypeHelper.ParseGroupRole(roleEle.GetString() ?? string.Empty);
            }
            Title = "Anonymous";
            if (je.TryGetProperty("title", out JsonElement titleEle))
            {
                Title = titleEle.GetString() ?? string.Empty;
            }
            //Card隐式指定为String.Empty
            Card = "";
            if (je.TryGetProperty("card", out JsonElement cardEle))
            {
                Card = cardEle.GetString() ?? string.Empty;
            }
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(rootJE), e);
        }
    }

    internal static CQGroupMessageSender GetFrom(JsonElement je)
    {
        CQGroupMessageSender gmsf = new();
        gmsf.LoadFrom(je);
        return gmsf;
    }

}
