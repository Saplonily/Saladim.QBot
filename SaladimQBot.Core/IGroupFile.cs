namespace SaladimQBot.Core;

/// <summary>
/// 一个Q群的文件
/// </summary>
public interface IGroupFile : IClientEntity
{
    /// <summary>
    /// 文件Id
    /// </summary>
    public string FileId { get; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; }
}
