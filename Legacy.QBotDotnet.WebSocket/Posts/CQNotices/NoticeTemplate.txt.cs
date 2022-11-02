using System.Text.Json;

namespace QBotDotnet.CqWebSocket.Internal;
public class 名字 : CQNoticePost
{
    public long GroupId { get; protected set; }
    public long UserId { get; protected set; }
    internal protected 名字() { }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        GroupNoticeHelper.LoadFrom(je, out var groupId, out var userId);
        this.GroupId = groupId; this.UserId = userId;
        try
        {

        }
        catch (KeyNotFoundException)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je));
        }
    }
    internal static 名字 GetFrom(JsonElement je)
    {
        var post = new 名字();
        post.LoadFrom(je);
        return post;
    }
}