using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;
public class CQFriendFileNoticePost : CQNoticePost
{
    public long UserId { get; protected set; }
    public long FileSize { get; protected set; }
    public string FileName { get; protected set; } = string.Empty;
    public string FileUrl { get; protected set; } = string.Empty;
    internal protected CQFriendFileNoticePost() { }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            UserId = je.GetProperty("user_id").GetInt64();
            var fileJE = je.GetProperty("file");
            FileName = fileJE.GetProperty("name").GetString() ?? "";
            FileSize = fileJE.GetProperty("size").GetInt64();
            FileUrl = fileJE.GetProperty("url").GetString() ?? "";
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    internal static CQFriendFileNoticePost GetFrom(JsonElement je)
    {
        var post = new CQFriendFileNoticePost();
        post.LoadFrom(je);
        return post;
    }
}