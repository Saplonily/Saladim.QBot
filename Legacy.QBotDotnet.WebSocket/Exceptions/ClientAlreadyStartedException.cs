namespace QBotDotnet.Exceptions;

public class ClientAlreadyStartedException : Exception
{
    public override string Message { get => "The Sll.QBotDotnet client has already started!"; }
}