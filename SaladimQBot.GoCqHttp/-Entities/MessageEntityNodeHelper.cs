namespace SaladimQBot.GoCqHttp;

internal static class MessageEntityNodeHelper
{
    public static CqCodeType GetTypeFromString(string name)
    {
        try
        {
            return EnumAttributeCacher.GetEnumFromAttr<CqCodeType>(name);
        }
        catch (KeyNotFoundException)
        {
            return CqCodeType.Invalid;
        }
    }


    public static Type? FindClassFromCqCodeType(CqCodeType cqCode)
        => CqTypeMapper.FindClassForCqCodeType(cqCode);
}