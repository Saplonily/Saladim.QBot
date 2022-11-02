namespace QBotDotnet.Core;

public interface IMessageTextNode : IMessageEntityNode
{
    string Text { get; set; }
}