using System.Diagnostics;
using System.Text;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{DebuggerDisplay}")]
public class MessageChain : CqEntity, IMessageChain
{
    public List<MessageChainNode> MessageChainNodes { get; protected set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IReadOnlyList<IMessageChainNode> IMessageChain.ChainNodes => MessageChainNodes;

    public MessageChain(CqClient client, IEnumerable<MessageChainNode> nodes) : this(client)
    {
        MessageChainNodes.AddRange(nodes);
    }

    public MessageChain(CqClient client) : base(client)
    {
        MessageChainNodes = new();
    }

    internal static MessageChain FromModel(CqClient client, CqMessageChainModel model)
    {
        List<MessageChainNode> nodes = new();
        foreach (var nodeModel in model.ChainNodeModels)
        {
            var node = MessageChainNode.FromModel(client, nodeModel);
            if (node is not null)
                nodes.Add(node);
        }
        return new MessageChain(client, nodes);
    }

    internal CqMessageChainModel ToModel()
    {
        CqMessageChainModel chainModel = new();
        foreach (var node in this.MessageChainNodes)
        {
            chainModel.ChainNodeModels.Add(node.ToModel());
        }
        return chainModel;
    }

    private string DebuggerDisplay
    {
        get
        {
            StringBuilder sb = new();
            foreach (var node in MessageChainNodes)
            {
                if (node.NodeType is CqCodeType.Text)
                {
                    sb.Append(node.Cast<MessageChainTextNode>().Text);
                }
                else
                {
                    sb.Append('[');
                    sb.Append(node.NodeType);
                    sb.Append(']');
                }
            }
            return sb.ToString();
        }
    }
}
