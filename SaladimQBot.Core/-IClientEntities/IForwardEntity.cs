namespace SaladimQBot.Core;

public interface IForwardEntity : IClientEntity
{
    IReadOnlyList<IForwardNode> ForwardNodes { get; }
}
