using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("[Unimpl:{Name,nq}]")]
[JsonConverter(typeof(CqMessageUnimplementedNodeJsonConverter))]
public class CqMessageUnimplementedNode : CqMessageEntityNode, IMessageUnimplementedNode
{
    public override MessageNodeType NodeType { get => MessageNodeType.Unimplemented; }

    public IDictionary<string, string> Params { get; internal set; }

    public string Name { get; internal set; }

    public CqMessageUnimplementedNode(string name, IDictionary<string, string> @params)
        => (Name, Params) = (name, @params);

    public override string CqStringify()
    {
        StringBuilder sb = new();
        sb.Append("[CQ:");
        sb.Append(Name);
        sb.Append(',');
        var paramStrings = from param in Params
                           let name = param.Key
                           let value = MessageEntityHelper.CqEncode(param.Value)
                           select $"{name}={value}";
        sb.Append(string.Join(",", paramStrings));
        sb.Append(']');

        return sb.ToString();
    }
}