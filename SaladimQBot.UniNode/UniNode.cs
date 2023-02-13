using System.Text;

namespace SaladimQBot.Core;

/// <summary>
/// UniNode, 表示消息的一个节点
/// </summary>
public abstract class UniNode : IClientEntity
{
    public const char StartChar = '<';
    public const char EndChar = '>';
    public const char PrimaryKeyMarkChar = ':';
    public const char ArgSplitChar = ',';
    public const char ArgEqualChar = '=';

    /// <summary>
    /// 产生和管理该<see cref="UniNode"/>的<see cref="IClient"/>
    /// </summary>
    public IClient Client { get; protected set; }

    /// <summary>
    /// 该UniNode的名称
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// 主键名称, 没有主键时应为<see langword="null"/>
    /// </summary>
    public abstract string? PrimaryKey { get; }

    /// <summary>
    /// 主键对应的值, 没有主键时应为<see langword="null"/>
    /// </summary>
    public abstract string? PrimaryValue { get; }

    public UniNode(IClient client)
    {
        Client = client;
    }

    /// <summary>
    /// 将该Node解构为原始的键值对字典
    /// </summary>
    /// <returns></returns>
    public abstract IDictionary<string, string> Deconstruct();

    /// <summary>
    /// 解析为
    /// </summary>
    /// <returns></returns>
    public abstract string ToFormattedText();

    /// <summary>
    /// 转义一段文本, 防止其中的字符被语法解析
    /// </summary>
    /// <param name="str">要被转义的文本</param>
    /// <returns>转义后的文本</returns>
    public static string Escape(string str)
    {
        StringBuilder sb = new();
        foreach (char c in str)
        {
            string r = c switch
            {
                '\n' => @"\n",
                ' ' => @"\s",
                ',' => @"\c",
                '<' => @"\l",
                '>' => @"\r",
                '\\' => @"\\",
                _ => c.ToString()
            };
            sb.Append(r);
        }
        return sb.ToString();
    }

    /// <summary>
    /// 去除转义一段文本, 使文本从转义状态回到原本状态
    /// </summary>
    /// <param name="str">含转义的文本</param>
    /// <returns>去除转义后的文本</returns>
    public static string Deescape(string str)
    {
        StringBuilder sb = new();
        var ie = str.GetEnumerator();
        while (ie.MoveNext())
        {
            char cur = ie.Current;
            if (cur == '\\')
            {
                if (ie.MoveNext())
                {
                    char next = ie.Current;
                    char? deescapedChar = next switch
                    {
                        'n' => '\n',
                        's' => ' ',
                        'c' => ',',
                        'l' => '<',
                        'r' => '>',
                        '\\' => '\\',
                        _ => null
                    };
                    if (deescapedChar is null)
                    {
                        sb.Append(cur);
                        sb.Append(next);
                    }
                    else
                    {
                        sb.Append(deescapedChar);
                    }
                }
                else
                {
                    sb.Append(cur);
                }
            }
            else
            {
                sb.Append(ie.Current);
            }
        }
        return sb.ToString();
    }

}
