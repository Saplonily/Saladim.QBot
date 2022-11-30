using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class CqPostLoadFailedException : ClientException
{
    internal CqPostLoadFailedException(
        IClient client,
        string? message = null,
        CqPost? post = null,
        Exception? innerException = null
        )
        : base(client, ExceptionType.PostParsingFailed, GenerateString(post, message), innerException)
    {
    }

    protected static string GenerateString(CqPost? post, string? extraMessage)
    => $"CqPost load failed," + post is not null ?
        $"selfId: {post!.SelfId}, loaded PostType: {post!.PostType}"
        : $" no loaded CqPost instance provided. {extraMessage}";
}