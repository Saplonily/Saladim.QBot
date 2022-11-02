using System.Collections;
using System.Text.Json;

namespace QBotDotnet.GoCqHttp;

/// <summary>
/// 定义QQ中任何可能含有CQ码的信息/字符串
/// </summary>
public class CQMessage : IEnumerable, IEnumerable<CQMessagePart>
{
    public List<CQMessagePart> CQParts { get; internal set; } = new();
    public IEnumerator GetEnumerator() => CQParts.GetEnumerator();
    IEnumerator<CQMessagePart> IEnumerable<CQMessagePart>.GetEnumerator() => CQParts.GetEnumerator();
    internal static CQMessage GetFrom(JsonElement je)
    {
        CQMessage message = new();
        try
        {
            foreach (var item in je.EnumerateArray())
            {
                message.CQParts.Add(CQMessagePart.GetFrom(item));
            }
        }
        catch (InvalidOperationException e)
        {
            throw new Posts.CQPostTypeInvalidLoadException(nameof(je), e);
        }
        return message;
    }
}