using System.Drawing;
using System.Numerics;

namespace SaladimQBot.Extensions;

public static class CommonTypeParsers
{
    public static int Int(string s) => int.Parse(s);
    public static long Long(string s) => long.Parse(s);
    public static short Short(string s) => short.Parse(s);
    public static uint Uint(string s) => uint.Parse(s);
    public static ulong Ulong(string s) => ulong.Parse(s);
    public static ushort Ushort(string s) => ushort.Parse(s);
    public static byte Byte(string s) => byte.Parse(s);
    public static char Char(string s) => char.Parse(s);
    public static float Float(string s) => float.Parse(s);
    public static double Double(string s) => double.Parse(s);
    public static sbyte Sbyte(string s) => sbyte.Parse(s);
    public static Vector2 Vector2(string s)
    {
        //以逗号分隔
        string[] ps = s.Split(',', '，');
        if (ps.Length != 2) throw new CommonTypeParseFailedException();
        //允许括号
        if (ps[0].StartsWith("(") && ps[1].EndsWith(")"))
        {
            ps[0] = ps[0].Substring(1);
            ps[1] = ps[1].Substring(0, ps[1].Length - 1);
        }
        return new(Float(ps[0]), Float(ps[1]));
    }
    public static Vector3 Vector3(string s)
    {
        //以逗号分隔
        string[] ps = s.Split(',', '，');
        if (ps.Length != 3) throw new CommonTypeParseFailedException();
        //允许括号
        if (ps[0].StartsWith("(") && ps[2].EndsWith(")"))
        {
            ps[0] = ps[0].Substring(1);
            ps[2] = ps[2].Substring(0, ps[2].Length - 1);
        }
        return new(Float(ps[0]), Float(ps[1]), Float(ps[2]));
    }
    public static Color Color(string s)
    {
        //假设是以#开头的16进制形式
        if (s.StartsWith("#"))
        {
            string hex = s.Substring(1);
            //假设是 RRGGBB
            if (hex.Length == 6)
            {
                string rr = hex.Substring(0, 2);
                string gg = hex.Substring(2, 2);
                string bb = hex.Substring(4, 2);
                int irr = int.Parse(rr, System.Globalization.NumberStyles.HexNumber);
                int igg = int.Parse(gg, System.Globalization.NumberStyles.HexNumber);
                int ibb = int.Parse(bb, System.Globalization.NumberStyles.HexNumber);
                return System.Drawing.Color.FromArgb(255, irr, igg, ibb);
            }
            //假设是 RRGGBBAA
            else if (hex.Length == 8)
            {
                string rr = hex.Substring(0, 2);
                string gg = hex.Substring(2, 2);
                string bb = hex.Substring(4, 2);
                string aa = hex.Substring(6, 2);
                int irr = int.Parse(rr, System.Globalization.NumberStyles.HexNumber);
                int igg = int.Parse(gg, System.Globalization.NumberStyles.HexNumber);
                int ibb = int.Parse(bb, System.Globalization.NumberStyles.HexNumber);
                int iaa = int.Parse(aa, System.Globalization.NumberStyles.HexNumber);
                return System.Drawing.Color.FromArgb(iaa, irr, igg, ibb);
            }
            else
            {
                throw new CommonTypeParseFailedException();
            }
        }
        //假设是以逗号分隔的形式
        else if (s.Contains(',') || s.Contains('，'))
        {
            //不是小数的形式
            if (!s.Contains('.'))
            {
                string[] nums = s.Split(',', '，');
                if (nums.Length == 3)
                {
                    int r = int.Parse(nums[0]);
                    int g = int.Parse(nums[1]);
                    int b = int.Parse(nums[2]);
                    return System.Drawing.Color.FromArgb(255, r, g, b);
                }
                else if (nums.Length == 4)
                {
                    int r = int.Parse(nums[0]);
                    int g = int.Parse(nums[1]);
                    int b = int.Parse(nums[2]);
                    int a = int.Parse(nums[3]);
                    return System.Drawing.Color.FromArgb(a, r, g, b);
                }
                else
                {
                    throw new CommonTypeParseFailedException();
                }
            }
            //含小数点, 是小数形式
            else
            {
                string[] nums = s.Split(',', '，');
                if (nums.Length == 3)
                {
                    float r = float.Parse(nums[0]);
                    float g = float.Parse(nums[1]);
                    float b = float.Parse(nums[2]);
                    return System.Drawing.Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
                }
                else if (nums.Length == 4)
                {
                    float r = float.Parse(nums[0]);
                    float g = float.Parse(nums[1]);
                    float b = float.Parse(nums[2]);
                    float a = float.Parse(nums[3]);
                    return System.Drawing.Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
                }
                else
                {
                    throw new CommonTypeParseFailedException();
                }
            }
        }
        //最后假设是昵称形式
        else
        {
            var c = System.Drawing.Color.FromName(s);
            //获取失败了
            if (c.A == 0 && c.R == 0 && c.G == 0 && c.B == 0)
                throw new CommonTypeParseFailedException();
            return c;
        }
    }
    public static T[] ArrayPacker<T>(string s, Func<string, T> subParser, char spliter)
    {
        var subStrings = s.Split(spliter);
        if (subStrings.Length == 1) return new T[] { subParser(s) };
        T[] result = new T[subStrings.Length];
        for (int i = 0; i < subStrings.Length; i++)
        {
            result[i] = subParser(subStrings[i]);
        }
        return result;
    }

    public class CommonTypeParseFailedException : Exception
    {
    }
}