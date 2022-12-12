using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public abstract class MessageChainNode : CqEntity, IMessageChainNode
{
    public abstract CqCodeType NodeType { get; }

    MessageNodeType IMessageChainNode.NodeType => NodeType.Cast<MessageNodeType>();

    protected MessageChainNode(CqClient client) : base(client)
    {
    }

    internal static MessageChainNode? FromModel(CqClient client, CqMessageChainNodeModel model)
    {
        return model.CqCodeType switch
        {
            CqCodeType.Text =>
                new MessageChainTextNode(client, model.Params["text"]),

            CqCodeType.At =>
                //model.Params["qq"] != "all" ? 
                new MessageChainAtNode(
                    client,
                    long.Parse(model.Params["qq"]),
                    model.Params.TryGetValue("name", out var v) ? v : ""
                    ),

            CqCodeType.Face =>
                new MessageChainFaceNode(client, int.Parse(model.Params["id"])),

            CqCodeType.Image =>
                new MessageChainImageNode(client, model.Params["file"]),

            CqCodeType.Reply =>
                new MessageChainReplyNode(client, client.GetMessageById(int.Parse(model.Params["id"]))),

            CqCodeType.Forward =>
                new MessageChainForwardNode(client, new ForwardEntity(client, model.Params["id"])),

            _ =>
                new MessageChainUnimplementedNode(client, model.RawCqCodeName!, model.Params),
        };
    }

    internal abstract CqMessageChainNodeModel ToModel();
}
