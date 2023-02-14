using SaladimQBot.Core;

namespace SaladimQBot.Core;

public partial class GenericUniNode
{
    public static GenericUniNode Parse(IClient client, string str)
    {
        if (!TryParse(client, str, out var node))
            throw new FormatException("Invalid uniNode.");
        return node!;
    }

    public static bool TryParse(IClient client, string str, out GenericUniNode? outNode)
    {
        outNode = null;
        int strLength = str.Length;
        if (str[0] != StartChar || str[strLength - 1] != EndChar)
            return false;
        // <at:2748166392>
        // <image:114514.png>
        // <image,uri=1919.png>
        //从优化方面我们对TFM非std2.0使用Span<char>
#if !NETSTANDARD2_0
        var trimedStr = str.AsSpan(1, strLength - 2);
        int primaryKeySpan = trimedStr.IndexOf(PrimaryKeyMarkChar);
        int firstSplitCharInd = trimedStr.IndexOf(ArgSplitChar);
        int firstEqualCharInd = trimedStr.IndexOf(ArgEqualChar);
        if (primaryKeySpan != -1)
        {
            //有冒号, 带有主参
            var nameSpan = trimedStr[..primaryKeySpan];
            ReadOnlySpan<char> mainKeySpan;
            if (firstSplitCharInd != -1)
            {
                mainKeySpan = trimedStr[(primaryKeySpan + 1)..firstSplitCharInd];
                var argsSpan = trimedStr[(firstSplitCharInd + 1)..];
                if (TryParseArgString(argsSpan, out var dic))
                {
                    GenericUniNode node = new(client, nameSpan.ToString(), dic!, mainKeySpan.ToString());
                    outNode = node;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (firstEqualCharInd != -1) return false;
                mainKeySpan = trimedStr[(primaryKeySpan + 1)..];
                GenericUniNode node = new(client, nameSpan.ToString(), new Dictionary<string, string>(), mainKeySpan.ToString());
                outNode = node;
                return true;
            }
        }
        else
        {
            //无冒号, 无主参, 尝试寻找有没有其他参数
            if (firstEqualCharInd == -1 && firstSplitCharInd == -1)
            {
                //无'=', 无',', 假设这是一个自结束的UniNode
                GenericUniNode node = new(client, trimedStr.ToString(), new Dictionary<string, string>());
                outNode = node;
                return true;
            }
            else
            {
                if (firstSplitCharInd != strLength - 1 && firstSplitCharInd != 0)
                {
                    //可能含有 '=', ','之一, 那么扔到参数解析器里尝试解析一下
                    var nameSpan = trimedStr[..firstSplitCharInd];
                    var argsSpan = trimedStr[(firstSplitCharInd + 1)..];

                    if (TryParseArgString(argsSpan, out var dic))
                    {
                        GenericUniNode node = new(client, nameSpan.ToString(), dic!);
                        outNode = node;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
#else
        var trimedStr = str.Substring(1, strLength - 2);
        int primaryKeySpan = trimedStr.IndexOf(PrimaryKeyMarkChar);
        int firstSplitCharInd = trimedStr.IndexOf(ArgSplitChar);
        int firstEqualCharInd = trimedStr.IndexOf(ArgEqualChar);
        if (primaryKeySpan != -1)
        {
            //有冒号, 带有主参
            var nameSpan = trimedStr.Substring(0, primaryKeySpan);
            string mainKey;
            if (firstSplitCharInd != -1)
            {
                mainKey = trimedStr.Substring(primaryKeySpan + 1, firstSplitCharInd);
                var argsSpan = trimedStr.Substring(firstSplitCharInd + 1);
                if (TryParseArgString(argsSpan, out var dic))
                {
                    GenericUniNode node = new(client, nameSpan, dic!, mainKey);
                    outNode = node;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (firstEqualCharInd != -1) return false;
                mainKey = trimedStr.Substring(primaryKeySpan + 1);
                GenericUniNode node = new(client, nameSpan, new Dictionary<string, string>(), mainKey);
                outNode = node;
                return true;
            }
        }
        else
        {
            //无冒号, 无主参, 尝试寻找有没有其他参数
            if (firstEqualCharInd == -1 && firstSplitCharInd == -1)
            {
                //无'=', 无',', 假设这是一个自结束的UniNode
                GenericUniNode node = new(client, trimedStr, new Dictionary<string, string>());
                outNode = node;
                return true;
            }
            else
            {
                if (firstSplitCharInd != strLength - 1 && firstSplitCharInd != 0)
                {
                    //可能含有 '=', ','之一, 那么扔到参数解析器里尝试解析一下
                    var nameSpan = trimedStr.Substring(0, firstSplitCharInd);
                    var argsSpan = trimedStr.Substring(firstSplitCharInd + 1);

                    if (TryParseArgString(argsSpan, out var dic))
                    {
                        GenericUniNode node = new(client, nameSpan, dic!);
                        outNode = node;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
#endif
    }

#if !NETSTANDARD2_0
    //接收类似 'qq=2748166392' 或 'img=http://some.url,type=image' 的字串
    internal static bool TryParseArgString(ReadOnlySpan<char> charSpan, out Dictionary<string, string>? outDic)
    {
        Dictionary<string, string> dic = new();
        var allSpans = charSpan.Split(ArgSplitChar);
        foreach (var p in allSpans)
        {
            int equalCharPos = p.IndexOf(ArgEqualChar);
            if (equalCharPos is not -1 or 0 && equalCharPos != p.Length - 1)
            {
                var leftKey = p[..equalCharPos];
                var rightValue = p[(equalCharPos + 1)..];
                dic.Add(Deescape(leftKey.ToString()), Deescape(rightValue.ToString()));
            }
            else
            {
                outDic = null;
                return false;
            }
        }
        outDic = dic;
        return true;
    }
#else
    //接收类似 'qq=2748166392' 或 'img=http://some.url,type=image' 的字串
    internal static bool TryParseArgString(string chars, out Dictionary<string, string>? outDic)
    {
        Dictionary<string, string> dic = new();
        var allSpans = chars.Split(ArgSplitChar);
        foreach (var p in allSpans)
        {
            int equalCharPos = p.IndexOf(ArgEqualChar);
            if (equalCharPos is not -1 or 0 && equalCharPos != p.Length - 1)
            {
                var leftKey = p.Substring(0, equalCharPos);
                var rightValue = p.Substring(equalCharPos + 1);
                dic.Add(Deescape(leftKey), Deescape(rightValue));
            }
            else
            {
                outDic = null;
                return false;
            }
        }
        outDic = dic;
        return true;
    }
#endif

}