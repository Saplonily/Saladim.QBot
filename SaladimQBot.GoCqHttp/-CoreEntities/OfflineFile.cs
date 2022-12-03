using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

public class OfflineFile : CqEntity, IOfflineFile
{
    public string FileName { get; set; }

    public string FileUrl { get; set; }

    public long FileSize { get; set; }

    public string FileSizeString => NumberHelper.GetSizeString(FileSize);

    public OfflineFile(CqClient client, string fileName, string fileUrl, long fileSize) : base(client)
    {
        this.FileName = fileName;
        this.FileUrl = fileUrl;
        this.FileSize = fileSize;
    }

    public OfflineFile(CqClient client, CqOfflineFileUploadedNoticePost.FileEntity entity) : base(client)
    {
        this.FileName = entity.FileName;
        this.FileUrl = entity.Url;
        this.FileSize = entity.FileSize;
    }

    public override bool Equals(object? obj)
    {
        return obj is OfflineFile file &&
               this.FileName == file.FileName &&
               this.FileUrl == file.FileUrl &&
               this.FileSize == file.FileSize;
    }

    public bool Equals(IOfflineFile? other)
    {
        return this.Equals((object?)other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.FileName, this.FileUrl, this.FileSize);
    }

    public static bool operator ==(OfflineFile? left, OfflineFile? right)
    {
        return EqualityComparer<OfflineFile>.Default.Equals(left, right);
    }

    public static bool operator !=(OfflineFile? left, OfflineFile? right)
    {
        return !(left == right);
    }
}
