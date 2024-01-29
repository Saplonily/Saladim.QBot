using System.Text;

namespace SaladimQBot.GoCqHttp;

public static class MessageChainModelHelper
{
    public static CqMessageChainModel RawStringToChain(string rawString)
    {
        //TODO : RawStringToChain
        throw new NotImplementedException();
    }

    public static string ChainToRawString(CqMessageChainModel entity)
    {
        StringBuilder sb = new();
        foreach (var node in entity.ChainNodeModels)
        {
            sb.Append(node.CqStringify());
        }
        return sb.ToString();
    }

    internal static string CqStringify(this CqMessageChainNodeModel node) => node.CqCodeType switch
    {
        CqCodeType.Text => CqEncode(node.Params["text"]),
        CqCodeType.Unimplemented => ParamsDicToCqString(
            node.Params,
            node.RawCqCodeName ??
            throw new InvalidOperationException("Unimplemented MessageChainNodeModel must have rawCqCodeName")
            ),
        _ => ParamsDicToCqString(node.Params, node.CqCodeType)
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

    public static ForwardEntity GetForwardEntity(CqClient client, string forwardId)
    {
        throw new NotImplementedException();
    }
}