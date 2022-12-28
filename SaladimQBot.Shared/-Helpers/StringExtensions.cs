namespace SaladimQBot.Shared;

public static class StringExtensions
{
    public static int Count(this string stringToBeCount, string subString)
    {
        return (stringToBeCount.Length - stringToBeCount.Replace(subString, "").Length) / subString.Length;
    }
}
