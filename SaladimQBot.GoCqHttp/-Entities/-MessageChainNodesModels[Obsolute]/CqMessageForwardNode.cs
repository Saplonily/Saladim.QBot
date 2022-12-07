using System.Text.Json.Serialization;
using SaladimQBot.Core;
using SaladimQBot.Shared;

//TODO 重构Node

namespace SaladimQBot.GoCqHttp;

public class CqMessageForwardReceiveNode : CqEntity
{
    [Name("id")]
    public string ForwardId { get; protected set; }

    [Ignore]
    public MessageNodeType NodeType => CqCodeType.Forward.Cast<MessageNodeType>();

    [Ignore]
    public Expirable<ForwardEntity> ForwardEntity { get; protected set; }

    public CqMessageForwardReceiveNode(CqClient client, string forwardId) : base(client)
    {
        ForwardId = forwardId;
        ForwardEntity = client.MakeNoneExpirableExpirable(() => MessageChainHelper.GetForwardEntity(client, forwardId));
    }

    IForwardEntity IMessageForwardReceiveNode.ForwardEntity => ForwardEntity.Value;
}
