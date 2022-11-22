namespace SaladimQBot.Core;

public interface IMessageTextNode : IMessageEntityNode
{
    string Text { get; set; }
}