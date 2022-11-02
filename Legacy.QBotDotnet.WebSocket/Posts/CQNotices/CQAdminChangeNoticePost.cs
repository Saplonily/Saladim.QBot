using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

/// <summary>
/// 管理员变更Post
/// </summary>
public class CQAdminChangeNoticePost : CQNoticePost
{
    public long GroupId { get; protected set; }
    public long UserId { get; protected set; }
    public bool? IsSet { get; protected set; }
    internal protected CQAdminChangeNoticePost() { }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        GroupNoticeHelper.LoadFrom(je, out var groupId, out var userId);
        GroupId = groupId; UserId = userId;
        bool s, u;
        try
        {
            s = je.GetPropertyWithExData("sub_type").GetString() == "set";
            u = je.GetPropertyWithExData("sub_type").GetString() == "unset";
        }
        catch (KeyNotFoundException e)
        {
            throw new Exceptions.PostLoadFailedException(nameof(je), text: e.Data[JsonHelper.StringKey] as string);

        }
        if (s == u) throw new CQPostTypeInvalidLoadException(nameof(je), text: "sub_type is not one of [set,unset]");
        IsSet = s;
    }
    internal static CQAdminChangeNoticePost GetFrom(JsonElement je)
    {
        var post = new CQAdminChangeNoticePost();
        post.LoadFrom(je);
        return post;
    }
}