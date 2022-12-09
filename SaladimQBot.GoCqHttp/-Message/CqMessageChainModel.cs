using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

[JsonConverter(typeof(CqMessageChainModelJsonConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CqMessageChainModel
{
    internal List<CqMessageChainNodeModel> ChainNodeModels = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            StringBuilder sb = new();
            foreach (var item in ChainNodeModels)
            {
                switch (item.CqCodeType)
                {
                    case CqCodeType.Text:
                        sb.Append(item.Params[MessageChainTextNode.TextPropertyName]);
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