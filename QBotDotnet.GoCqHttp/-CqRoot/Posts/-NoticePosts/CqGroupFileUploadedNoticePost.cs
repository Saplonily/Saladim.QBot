using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Posts;

public partial class CqGroupFileUploadedNoticePost : CqGroupUserNoticePost
{
    [Name("file")]
    public FileEntity File { get; set; } = default!;
}

public partial class CqGroupFileUploadedNoticePost
{
    public class FileEntity
    {
        [Name("id")]
        public string Id { get; set; } = default!;
        [Name("name")]
        public string Name { get; set; } = default!;
        [Name("size")]
        public Int64 Size { get; set; }
        [Name("busid")]
        public Int64 BusId { get; set; }

        public FileEntity() { }
        [JsonConstructor]
        public FileEntity(string id, string name, long size, long busId)
            => (Id, Name, Size, BusId) = (id, name, size, busId);
    }
}