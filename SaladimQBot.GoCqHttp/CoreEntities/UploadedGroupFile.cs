using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

public class UploadedGroupFile : CqEntity, IUploadedGroupFile
{
    public string FileId { get; protected set; }

    public string FileName { get; protected set; }

    public long FileSize { get; protected set; }

    public string FileSizeString => NumberHelper.GetSizeString(FileSize);

    public UploadedGroupFile(CqClient client, string fileId, string fileName, long fileSize) : base(client)
    {
        this.FileId = fileId;
        this.FileName = fileName;
        this.FileSize = fileSize;
    }

    public UploadedGroupFile(CqClient client, CqGroupFileUploadedNoticePost.FileEntity fileEntity) : base(client)
    {
        this.FileId = fileEntity.Id;
        this.FileName = fileEntity.Name;
        this.FileSize = fileEntity.Size;
    }

    public override bool Equals(object? obj)
    {
        return obj is UploadedGroupFile file &&
               this.FileId == file.FileId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.FileId);
    }

    public static bool operator ==(UploadedGroupFile? left, UploadedGroupFile? right)
    {
        return EqualityComparer<UploadedGroupFile>.Default.Equals(left!, right!);
    }

    public static bool operator !=(UploadedGroupFile? left, UploadedGroupFile? right)
    {
        return !(left == right);
    }
}
