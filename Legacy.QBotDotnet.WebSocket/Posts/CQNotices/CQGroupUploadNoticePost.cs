using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGroupUploadNoticePost : CQNoticePost
{
    public long GroupId { get; protected set; }
    public long SenderId { get; protected set; }
    public string FileId { get; protected set; } = string.Empty;
    public string FileName { get; protected set; } = string.Empty;
    public long FileSize { get; protected set; } = -1;
    public long FileBusId { get; protected set; } = -1;
    public string? FileUrl { get; protected set; } = null;

    internal static CQGroupUploadNoticePost GetFrom(JsonElement je)
    {
        CQGroupUploadNoticePost post = new();
        post.LoadFrom(je);
        return post;
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        GroupNoticeHelper.LoadFrom(je, out long groupId, out long userId);
        GroupId = groupId; SenderId = userId;
        try
        {
            var fileJE = je.GetProperty("file");
            FileId = fileJE.GetProperty("id").GetString() ?? "";
            FileName = fileJE.GetProperty("name").GetString() ?? "";
            FileSize = fileJE.GetProperty("size").GetInt64();
            FileBusId = fileJE.GetProperty("busid").GetInt64();
            //go-cqhttp文档中并没有url的上报说明
            //但是实际上报有url字段,安全起见FileUrl可null
            if (fileJE.TryGetProperty("url", out var urlJE))
            {
                FileUrl = urlJE.GetString();
            }
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
}