using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using QBotDotnet.Core;
using QBotDotnet.GoCqHttp;

namespace QBotDotnet.GoCqHttp;

//MessageEntity指定通过GetMessageNodes()实现了IEnumerable接口
//所以会被序列化器序列化为Array
//所以无需添加Ignore等特性指定
[JsonConverter(typeof(CqMessageEntityJsonConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CqMessageEntity : List<CqMessageEntityNode>, IMessageEntity
{
    IEnumerator<IMessageEntityNode> IEnumerable<IMessageEntityNode>.GetEnumerator()
        => GetEnumerator();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]

    private string DebuggerDisplay
    {
        get
        {
            StringBuilder sb = new();
            foreach (var item in this)
            {
                switch (item)
                {
                    case CqMessageTextNode textNode:
                        sb.Append(textNode.Text);
                        break;
                    case CqMessageAtNode atNode:
                        sb.Append($"[at:{atNode.UserIdStr}]");
                        break;
                    default:
                        sb.Append('[')
                              .Append(item.NodeType)
                              .Append(']');
                        break;
                }
            }
            return sb.ToString();
        }
    }
}