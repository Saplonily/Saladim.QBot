namespace QBotDotnet.GoCqHttp;

public class CqPostLoadFailedExcpetion : Exception
{
    protected static string GenerateString(CqPost? post)
    => $"CqPost load failed," + post is not null ?
        $"selfId: {post!.SelfId}, loaded PostType: {post!.PostType}"
        : $" no loaded CqPost instance provided.";

    public CqPostLoadFailedExcpetion(CqPost? post) : base(GenerateString(post))
    {

    }
}