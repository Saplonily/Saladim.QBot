namespace SaladimQBot.Core;

public interface IOfflineFile : IClientEntity
{
    string FileName { get; }

    string FileUrl { get; }

    long FileSize { get; }
}
