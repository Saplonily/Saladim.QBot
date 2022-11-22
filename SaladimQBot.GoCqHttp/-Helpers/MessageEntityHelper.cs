using System.Text;
using System.Web;

namespace SaladimQBot.GoCqHttp;

using static EnumAttributeCacher;

public static class MessageEntityHelper
{
    public static CqMessageEntity RawString2CqEntity(string rawString)
    {
        throw new NotImplementedException();
    }

    public static string CqEntity2RawString(CqMessageEntity entity)
    {
        StringBuilder sb = new();
        foreach (var node in entity)
        {
            sb.Append(node.CqStringify());
        }
        return sb.ToString();


    }

    private static string GetCqCodeName(CqCodeType type)
        => (string)GetAttrFromEnum(typeof(CqCodeType), (int)type);

    internal static string GetParaValuePair(string argName, string arg)
        => $"{argName}={CqEncode(arg)}";

    public static string CqEncode(string str)
        => str.Replace("&", "&amp;")
              .Replace("[", "&#91;")
              .Replace("]", "&#93;")
              .Replace(",", "&#44;");

    public static string CqDecode(string str)
        => str.Replace("&amp;", "&")
              .Replace("&#91;", "[")
              .Replace("&#93;", "]")
              .Replace("&#44;", ",");

    internal static void AppendCqHead(this StringBuilder sb, CqMessageEntityNode node)
    {
        sb.Append('[');
        sb.Append("CQ:");
        sb.Append(GetCqCodeName(node.CqCodeType));
        sb.Append(',');
    }

    internal static void AppendCqFoot(this StringBuilder sb)
    {
        sb.Append(']');
    }
}