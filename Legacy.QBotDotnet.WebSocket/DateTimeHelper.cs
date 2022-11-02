namespace QBotDotnet.Misc;

public static class DateTimeHelper
{
    public static DateTime GetFromUnix(long unixTime)
    {
        return (new DateTime(1970, 1, 1)).AddSeconds(unixTime).ToLocalTime();
    }
}