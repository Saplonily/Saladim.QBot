namespace SaladimQBot.GoCqHttp.Posts;

public class CqOfflineFileUploadedNoticePost : CqNoticePost
{
    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("file")]
    public FileEntity File { get; set; } = default!;

    public class FileEntity
    {
        [Name("name")]
        public string FileName { get; set; } = default!;

        [Name("url")]
        public string Url { get; set; } = default!;

        [Name("size")]
        public Int64 FileSize { get; set; }
    }
}