using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQPost
{
    public long Time { get; private set; }
    public long SelfId { get; private set; }
    public CQPostType PostType { get; private set; }
    public DateTime DateTime { get => DateTimeHelper.GetFromUnix(Time); }

    protected CQPost() { }
    internal virtual void LoadFrom(JsonElement rootJE)
    {
        try
        {
            Time = rootJE.GetProperty("time").GetInt64();
            SelfId = rootJE.GetProperty("self_id").GetInt64();
            PostType = rootJE.GetProperty("post_type").GetString() switch
            {
                "message" => CQPostType.Message,
                "request" => CQPostType.Request,
                "notice" => CQPostType.Notice,
                "meta_event" => CQPostType.MetaEvent,
                _ => CQPostType.Invalid
            };
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(rootJE), e);
        }
    }
    /// <summary>
    /// 从一个Json元素中获取CQPost对象
    /// </summary>
    /// <param name="rootJE">对应的根Json元素</param>
    /// <param name="doUpdate">是否根据内容进行升级为CQMessagePost等子类Post</param>
    /// <returns></returns>
    internal static CQPost GetFrom(JsonElement rootJE, bool doUpdate)
    {
        CQPost cqPost = new();
        cqPost.LoadFrom(rootJE);
        return !doUpdate ? cqPost : cqPost.PostType switch
        {
            CQPostType.Message => CQMessagePost.GetFrom(rootJE, doUpdate),
            CQPostType.Request => CQRequestPost.GetFrom(rootJE, doUpdate),
            CQPostType.Notice => CQNoticePost.GetFrom(rootJE, doUpdate),
            //TODO: Metaevent
            _ => cqPost,
        };
    }

    public enum CQPostType
    {
        Invalid,
        Message,
        Request,
        Notice,
        MetaEvent
    }
}