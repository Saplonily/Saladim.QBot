using System.Text.Json;
using System.Text.RegularExpressions;

namespace Saladim.Offbot.Services;

public partial class HomoService
{
    public static readonly Dictionary<double, string> RecordOfTabledNumbers;

    public static readonly IEnumerable<double> TabledNumbers;

    private static readonly Regex PutInExpressionRegex = GetPutInExpressionRegex();
    /// <summary>
    /// 去乘除一个只有乘除没有括号的式子时可去除括号
    /// </summary>
    private static readonly Regex BraceRemoveTimesDivideTypeRegex = GetBraceRemoveTimesDivideTypeRegex();
    /// <summary>
    /// 一个式子如果里面没括号且外边只有+-时可去除括号
    /// </summary>
    private static readonly Regex BraceRemoveAddSubstractOnlyTypeRegex = GetBraceRemoveAddSubstractOnlyTypeRegex();
    /// <summary>
    /// 加减一个处于末尾的表达式可以去除括号
    /// </summary>
    private static readonly Regex BraceRemoveExpressionAtTheEndRegex = GetBraceRemoveExpressionAtTheEndRegex();

    [GeneratedRegex(@"\d+|\(-1\)", RegexOptions.Compiled)]
    private static partial Regex GetPutInExpressionRegex();

    [GeneratedRegex(@"([\*\/])\(([^\+\-\(\)]+)\)", RegexOptions.Compiled)]
    private static partial Regex GetBraceRemoveTimesDivideTypeRegex();

    [GeneratedRegex(@"([\+\-])\(([^\(\)]+)\)([\+\-\)])", RegexOptions.Compiled)]
    private static partial Regex GetBraceRemoveAddSubstractOnlyTypeRegex();

    [GeneratedRegex(@"([\+|\-])\(([^\(\)]+)\)$", RegexOptions.Compiled)]
    private static partial Regex GetBraceRemoveExpressionAtTheEndRegex();

    static HomoService()
    {
        JsonDocument document = JsonDocument.Parse(File.ReadAllText("HomoNumberTable.json"));
        var dic = document.Deserialize<Dictionary<double, string>>();
        if (dic is null)
            throw new Exception("Can't deserialize HomoNumberTable.json to a dictionary.");
        RecordOfTabledNumbers = dic;
        TabledNumbers = dic.Keys;
    }

    public string Homo(double num)
    {
        if (double.IsInfinity(num) && double.IsNaN(num))
            return $"这么恶臭的{num}有必要论证吗";
        return Finisher(BreakNumber(num));
    }

    private string BreakNumber(double num)
    {
        if (double.IsInfinity(num) || double.IsNaN(num))
            throw new Exception();

        // 数字小于0, 前面乘个-1然后返回正数打碎后的式子
        if (num < 0)
        {
            return Trimer($"(-1)*({BreakNumber(num * -1)})");
        }
        // 非整数
        if (!double.IsInteger(num))
        {
            static int GetAfterPointDigitsCount(double d)
            {
                string str = d.ToString();
                return str.Length - str.IndexOf('.') - 1;
            }
            // n为小数点后有效数字个数
            int n = GetAfterPointDigitsCount(num);
            return Trimer($"({BreakNumber(num * Math.Pow(10, n))})/(10)^({n})".Replace("^(1)", ""));
        }

        // 表内已有值直接返回
        if (TabledNumbers.Any(n => n == num))
            return Trimer(num is -1 ? "(-1)" : num.ToString());
        var ie = TabledNumbers.GetEnumerator();

        double maximumRecordedNum = TabledNumbers.First(n => n < num);

        // 将不在表内的数字转换成已有数字的表达式, 具体算法是个数学问题, 可以自己琢磨琢磨
        string rst = $"{maximumRecordedNum}*({BreakNumber(Math.Floor((double)num / maximumRecordedNum))})+" +
            $"({BreakNumber(num % maximumRecordedNum)})";

        //把所有 *(1) 和 +(0) 和 ^(1) 这种无意义式子去掉
        return Trimer(rst);
        static string Trimer(string exp) => exp.Replace("*(1)", string.Empty).Replace("+(0)", string.Empty).Replace("^(1)", string.Empty);
    }

    private static string Finisher(string exp)
    {
        string expression = exp;
        {
            if (PutInExpressionRegex.IsMatch(exp))
            {
                static string Evaluator(Match m)
                {
                    return m.Value != "(-1)" ? RecordOfTabledNumbers[double.Parse(m.ValueSpan)] : $"({RecordOfTabledNumbers[-1]})";
                }
                expression = PutInExpressionRegex.Replace(expression, Evaluator);
            }
        }
        expression = expression.Replace("^", "**");
        {
            while (BraceRemoveTimesDivideTypeRegex.IsMatch(expression))
                expression = BraceRemoveTimesDivideTypeRegex
                    .Replace(expression, m => m.Groups[1].Value + m.Groups[2].Value);
            while (BraceRemoveAddSubstractOnlyTypeRegex.IsMatch(expression))
                expression = BraceRemoveAddSubstractOnlyTypeRegex
                    .Replace(expression, m => m.Groups[1].Value + m.Groups[2].Value + m.Groups[3].Value);
            while (BraceRemoveExpressionAtTheEndRegex.IsMatch(expression))
                expression = BraceRemoveExpressionAtTheEndRegex
                    .Replace(expression, m => m.Groups[1].Value + m.Groups[2].Value);
        }
        if (expression.StartsWith('(') && expression.EndsWith(')'))
        {
            var trimed = expression.AsSpan()[1..^1];
            if (!trimed.Contains('(') && !trimed.Contains(')'))
                expression = expression[1..^1];
        }
        expression = expression.Replace("+-", "-");
        return expression;
    }
}