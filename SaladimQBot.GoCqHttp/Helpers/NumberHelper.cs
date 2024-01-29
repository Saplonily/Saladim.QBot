namespace SaladimQBot.GoCqHttp;

public static class NumberHelper
{
    public static readonly string[] SizeSuffixes = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };

    public static string GetSizeString(int num)
        => GetSizeString((long)num);

    public static string GetSizeString(long num)
    {
        int level = 0; double fileSizeInShort = num;
        while (fileSizeInShort >= 1024)
        {
            fileSizeInShort /= 1024;
            level++;
        }
        return $"{fileSizeInShort:F2}{SizeSuffixes[level]}";
    }
}
