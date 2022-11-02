namespace QBotDotnet.GoCqHttp;

internal static class MessageEntityNodeHelper
{
    public static CqCodeType GetTypeFromString(string name)
        => EnumAttributeCacher.GetEnumFromAttr<CqCodeType>(name);


    public static Type? FindClassFromCqCodeType(CqCodeType cqCode)
        => CqTypeMapper.FindClassFromCqCodeType(cqCode);
}