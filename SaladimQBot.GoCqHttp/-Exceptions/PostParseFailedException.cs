namespace SaladimQBot.GoCqHttp;


public class PostParseFailedException : Exception
{
    public PostParseFailedException(Exception? innerException = null) : base(string.Empty, innerException)
    {
    }
}