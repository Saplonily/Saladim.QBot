using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class MessageChainForwardNode : MessageChainNode, IMessageChainForwardNode
{
    public override CqCodeType NodeType => CqCodeType.Forward;

    public ForwardEntity ForwardEntity { get; protected set; }

    public MessageChainForwardNode(CqClient client, ForwardEntity forwardEntity) : base(client)
    {
        this.ForwardEntity = forwardEntity;
    }

    internal override CqMessageChainNodeModel ToModel()
    {
        if (ForwardEntity.IsFromFlag)
        {
            StringDictionary dic = new()
            {
                ["id"] = ForwardEntity.Flag!
            };
            return new(CqCodeType.Forward, dic);
        }
        else
        {
            throw new InvalidOperationException("ContentForwardNode is not allowed to contain in the MessageChain.");
        }
    }

    IForwardEntity IMessageChainForwardNode.ForwardEntity => ForwardEntity;
}
