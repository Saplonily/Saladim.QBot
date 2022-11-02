﻿using System.Diagnostics;
using System.Text.Json.Serialization;
using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;

[DebuggerDisplay("[Unimpl:{Name,nq}]")]
[JsonConverter(typeof(CqMessageUnimplementedNodeJsonConverter))]
public class CqMessageUnimplementedNode : CqMessageEntityNode, IMessageUnimplementedNode
{
    public override MessageNodeType NodeType { get => MessageNodeType.Unimplemented; }
    public IDictionary<string, string> Params { get; internal set; }
    public string Name { get; internal set; }

    public CqMessageUnimplementedNode(string name, IDictionary<string, string> @params)
        => (Name, Params) = (name, @params);
}