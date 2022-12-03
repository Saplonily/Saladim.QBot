namespace SaladimQBot.Core;

public interface IOfflineFile
{
    string FileName { get; }

    string FileUrl { get; }

    long FileSize { get; }
}
