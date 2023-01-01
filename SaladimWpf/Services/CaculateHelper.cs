namespace SaladimWpf.Services;

/*
    排列组合部分来源: https://www.cnblogs.com/zhao-yi/p/8533035.html
*/

public class CaculateHelper
{
    public class Game1A2B
    {
        public static readonly char[] Game1A2BAllChars = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

        public static HashSet<char[]> Game1A2BGetAllPossibility(char[] nums, int sumCountOfAB)
        {
            if (sumCountOfAB == 0)
            {
                return CaculateHelper.GetPermutation(CopyArrayWithout(Game1A2BAllChars, nums), 4)!.ToHashSet();
            }
            int[] indexs = new int[] { 0, 1, 2, 3 };
            if (sumCountOfAB is < 0 or > 4)
                throw new InvalidOperationException($"{nameof(sumCountOfAB)} must in 0~4");
            List<int[]> indexsCombination = CaculateHelper.GetCombination(indexs, sumCountOfAB)!;
            List<char[]> allPermutation = new();
            for (int i = 0; i < indexsCombination.Count; i++)
            {
                char[] charsSpecified = CopyArrayWithIndexed(nums, indexsCombination[i]);
                char[] charsWithoutSpecified = CopyArrayWithout(Game1A2BAllChars, CopyArrayWithout(nums, charsSpecified));
                List<char[]> allSubPermutation = CaculateHelper.GetPermutation(charsWithoutSpecified, 4)!;
                allSubPermutation.RemoveAll(chars => !IsAllIn(chars, charsSpecified));
                allPermutation.AddRange(allSubPermutation);
            }
            return allPermutation.ToHashSet();

        }

        public static T[] CopyArrayWithoutIndex<T>(T[] array, int[] withoutsIndexs) where T : IEquatable<T>
        {
            List<T> things = new();
            for (int i = 0; i < array.Length; i++) if (!withoutsIndexs.Contains(i)) things.Add(array[i]);
            return things.ToArray();
        }

        public static T[] CopyArrayWithout<T>(T[] array, T[] withouts) where T : IEquatable<T>
        {
            List<T> things = new();
            for (int i = 0; i < array.Length; i++)
            {
                if (!withouts.Any(s => s.Equals(array[i])))
                {
                    things.Add(array[i]);
                }
            }
            return things.ToArray();
        }

        public static bool IsAllIn<T>(T[] arrayExpectToContainsAll, T[] subArray)
        {
            bool oneNotIn = false;
            for (int i = 0; i < subArray.Length; i++)
            {
                if (!arrayExpectToContainsAll.Contains(subArray[i]))
                {
                    oneNotIn = true;
                    break;
                }
            }
            return !oneNotIn;
        }

        public static T[] CopyArrayWithIndexed<T>(T[] array, int[] indexs)
        {
            List<T> things = new();
            for (int i = 0; i < array.Length; i++) if (indexs.Contains(i)) things.Add(array[i]);
            return things.ToArray();
        }
    }
    /// <summary>
    /// 交换两个变量
    /// </summary>
    /// <param name="a">变量1</param>
    /// <param name="b">变量2</param>
    public static void Swap<T>(ref T a, ref T b) => (b, a) = (a, b);

    /// <summary>
    /// 递归算法求数组的组合(私有成员)
    /// </summary>
    /// <param name="list">返回的范型</param>
    /// <param name="t">所求数组</param>
    /// <param name="n">辅助变量</param>
    /// <param name="m">辅助变量</param>
    /// <param name="b">辅助数组</param>
    /// <param name="M">辅助变量M</param>
    private static void GetCombination<T>(ref List<T[]> list, T[] t, int n, int m, int[] b, int M)
    {
        for (int i = n; i >= m; i--)
        {
            b[m - 1] = i - 1;
            if (m > 1)
            {
                GetCombination(ref list, t, i - 1, m - 1, b, M);
            }
            else
            {
                list ??= new List<T[]>();
                T[] temp = new T[M];
                for (int j = 0; j < b.Length; j++)
                {
                    temp[j] = t[b[j]];
                }
                list.Add(temp);
            }
        }
    }
    /// <summary>
    /// 递归算法求排列(私有成员)
    /// </summary>
    /// <param name="list">返回的列表</param>
    /// <param name="t">所求数组</param>
    /// <param name="startIndex">起始标号</param>
    /// <param name="endIndex">结束标号</param>
    private static void GetPermutation<T>(ref List<T[]> list, T[] t, int startIndex, int endIndex)
    {
        if (startIndex == endIndex)
        {
            list ??= new List<T[]>();
            T[] temp = new T[t.Length];
            t.CopyTo(temp, 0);
            list.Add(temp);
        }
        else
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                Swap(ref t[startIndex], ref t[i]);
                GetPermutation(ref list, t, startIndex + 1, endIndex);
                Swap(ref t[startIndex], ref t[i]);
            }
        }
    }
    /// <summary>
    /// 求从起始标号到结束标号的排列，其余元素不变
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="startIndex">起始标号</param>
    /// <param name="endIndex">结束标号</param>
    /// <returns>从起始标号到结束标号排列的范型</returns>
    public static List<T[]>? GetPermutation<T>(T[] t, int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex > t.Length - 1)
        {
            return null;
        }
        List<T[]> list = new();
        GetPermutation(ref list, t, startIndex, endIndex);
        return list;
    }
    /// <summary>
    /// 返回数组所有元素的全排列
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <returns>全排列的范型</returns>
    public static List<T[]> GetPermutation<T>(T[] t)
    {
        return GetPermutation(t, 0, t.Length - 1)!;
    }
    /// <summary>
    /// 求数组中n个元素的排列
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="n">元素个数</param>
    /// <returns>数组中n个元素的排列</returns>
    public static List<T[]>? GetPermutation<T>(T[] t, int n)
    {
        if (n > t.Length)
        {
            return null;
        }
        List<T[]> list = new();
        List<T[]> c = GetCombination(t, n)!;
        for (int i = 0; i < c.Count; i++)
        {
            List<T[]> l = new();
            GetPermutation(ref l, c[i], 0, n - 1);
            list.AddRange(l);
        }
        return list;
    }
    /// <summary>
    /// 求数组中n个元素的组合
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="n">元素个数</param>
    /// <returns>数组中n个元素的组合的范型</returns>
    public static List<T[]>? GetCombination<T>(T[] t, int n)
    {
        if (t.Length < n)
        {
            return null;
        }
        int[] temp = new int[n];
        List<T[]> list = new();
        GetCombination(ref list, t, t.Length, n, temp, n);
        return list;
    }
}