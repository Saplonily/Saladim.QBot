namespace SaladimQBot.Shared;

public static class DateTimeHelper
{
    public static DateTime GetFromUnix(long unixTime)
        => new DateTime(1970, 1, 1).AddSeconds(unixTime).ToLocalTime();

    public static DateTime GetFromUnix(uint unixTime)
        => GetFromUnix((long)unixTime);

    public static long ToUnix(DateTime dateTime)
    {
        return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
    }
}