namespace SaladimQBot.Core;

public interface IMessageChainTextNode : IMessageChainNode
{
    string Text { get; }
}