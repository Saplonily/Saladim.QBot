using System.Text;

namespace SaladimQBot.GoCqHttp;

public static class MessageChainHelper
{
    public static CqMessageChain RawStringToChain(string rawString)
    {
        throw new NotImplementedException();
    }

    public static string ChainToRawString(CqMessageChain entity)
    {
        StringBuilder sb = new();
        foreach (var node in entity)
        {
            sb.Append(node.CqStringify());
        }
        return sb.ToString();
    }

    internal static string CqStringify(this CqMessageEntityNode node) => node switch
    {
        CqMessageTextNode textNode => CqEncode(textNode.Text),
        CqMessageUnimplementedNode unimplNode => ParamsDicToCqString(unimplNode.Params, unimplNode.Name),
        _ => ParamsDicToCqString(node.GetParamsDictionary(), node.CqCodeType)
    };

    internal static string ParamsDicToCqString(IDictionary<string, string> paramDic, string cqCodeName)
    {
        var paramStrings =
                    from param in paramDic
                    where param.Value is not null
                    let key = param.Key
                    let value = CqEncode(param.Value)
                    select $"{key}={value}";
        var @params = string.Join(",", paramStrings);
        return $"[CQ:{cqCodeName},{@params}]";
    }

    internal static string ParamsDicToCqString(IDictionary<string, string> paramDic, CqCodeType cqCodeType)
        => ParamsDicToCqString(paramDic, EnumAttributeCacher.GetStrAttrFromEnum(cqCodeType));

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
}