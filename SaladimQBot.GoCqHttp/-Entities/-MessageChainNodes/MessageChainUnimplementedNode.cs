using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class MessageChainUnimplementedNode : MessageChainNode, IMessageChainUnimplementedNode
{
    public override CqCodeType NodeType => CqCodeType.Unimplemented;

    public IDictionary<string, string> Params { get; }

    public string NodeName { get; }

    public MessageChainUnimplementedNode(CqClient client, string nodeName, IDictionary<string, string> @params)
        : base(client)
    {
        Params = @params;
        NodeName = nodeName;
    }

    internal override CqMessageChainNodeModel ToModel()
    {
        CqMessageChainNodeModel m = new(NodeType, Params);
        m.RawCqCodeName = NodeName;
        return m;
    }
}
