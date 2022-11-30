using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[JsonConverter(typeof(CqMessageChainJsonConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CqMessageChain : List<CqMessageEntityNode>
{
    internal static CqMessageChain FromIMessageEntity(IMessageEntity entity)
    {
        if (entity is MessageEntity m) return m.cqChainEntity;
        CqMessageChain cqEntity = new();
        foreach (var node in entity)
        {
            switch (node.NodeType)
            {
                case MessageNodeType.Text:
                    var tNode = (IMessageTextNode)node;
                    cqEntity.Add(new CqMessageTextNode(tNode.Text));
                    break;

                case MessageNodeType.At:
                    var aNode = (IMessageAtNode)node;
                    cqEntity.Add(new CqMessageAtNode(aNode.UserId, aNode.UserName));
                    break;

                case MessageNodeType.Reply:
                    if (node is IMessageReplyIdNode rNode)
                        cqEntity.Add(new CqMessageReplyIdNode(rNode.MessageId));
                    else
                        //TODO: 支持其他reply节点
                        throw new NotImplementedException("other reply node is not supported for now.");
                    break;

                case MessageNodeType.Face:
                    var fNode = (IMessageFaceNode)node;
                    cqEntity.Add(new CqMessageFaceNode(fNode.FaceId));
                    break;

                case MessageNodeType.Image:
                    if (node is IMessageImageReceiveNode irNode)
                        cqEntity.Add(new CqMessageImageReceiveNode(
                            "",
                            irNode.File,
                            irNode.Type.Cast<ImageSendType>(),
                            irNode.SubType.Cast<ImageSendSubType>(),
                            irNode.ShowType.Cast<ImageShowType>()
                            ));
                    else
                        throw new NotImplementedException("other image node is not supported for now.");
                    break;

                case MessageNodeType.Unimplemented:
                    var unode = (IMessageUnimplementedNode)node;
                    cqEntity.Add(new CqMessageUnimplementedNode(unode.Name, unode.Params));
                    break;

                case MessageNodeType.Invalid:
                    throw new InvalidOperationException("Try to get an entity node from an invalid node type.");

                default:
                    throw new NotImplementedException("Other nodes are not supported yet. A issue to be raised plz.");
            }
        }
        return cqEntity;
    }

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