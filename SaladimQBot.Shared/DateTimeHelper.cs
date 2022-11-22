using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaladimQBot.Shared;

public static class DateTimeHelper
{
    public static DateTime GetFromUnix(long unixTime)
        => new DateTime(1970, 1, 1).AddSeconds(unixTime).ToLocalTime();

    public static DateTime GetFromUnix(uint unixTime)
        => GetFromUnix((long)unixTime);
}