using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class ForwardNode : IForwardNode
{
    public ForwardEntity ForwardEntity { get; protected set; }

    public ForwardNode(ForwardEntity entity)
    {
        ForwardEntity = entity;
    }

    IForwardEntity IForwardNode.ForwardEntity => ForwardEntity;
}