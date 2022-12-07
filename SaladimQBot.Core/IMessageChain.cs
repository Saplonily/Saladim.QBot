namespace SaladimQBot.Core;

public interface IMessageChain
{
    IReadOnlyList<IMessageChainNode> ChainNodes { get; }
}
