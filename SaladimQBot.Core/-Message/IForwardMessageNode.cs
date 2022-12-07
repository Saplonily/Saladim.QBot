namespace SaladimQBot.Core;

public interface IForwardMessageNode : IForwardNode
{
    IMessage Message { get; }
}
