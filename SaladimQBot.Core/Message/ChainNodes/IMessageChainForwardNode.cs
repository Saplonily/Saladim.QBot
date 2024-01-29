namespace SaladimQBot.Core;

public interface IMessageChainForwardNode : IMessageChainNode
{
    IForwardEntity ForwardEntity { get; }
}
