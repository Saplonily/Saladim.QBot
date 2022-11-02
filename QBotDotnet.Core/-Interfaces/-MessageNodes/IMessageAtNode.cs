namespace QBotDotnet.Core;

public interface IMessageAtNode : IMessageEntityNode
{
    long UserId { get; set; }
    string? UserName { get; set; }
}