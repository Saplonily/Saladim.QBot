namespace Saladim.Offbot.Services;

/*
    排列组合部分来源: https://www.cnblogs.com/zhao-yi/p/8533035.html
*/

public class CaculateHelper
{
    public class Game1A2B
    {
        public static readonly char[] Game1A2BAllChars = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

        public static HashSet<string> Game1A2BGetAllPossibility(char[] nums, int countOfA, int countOfB)
        {
            int[] indexs = new int[] { 0, 1, 2, 3 };
            if (countOfA + countOfB is < 0 or > 4)
                throw new InvalidOperationException($"sum of A and B must in 0~4");
            if (countOfA == 0 && countOfB == 0)
            {
                return GetPermutation(CopyArrayWithout(Game1A2BAllChars, nums), 4)!.Select(chars => new string(chars))!.ToHashSet();
            }
            List<string> charsAllPermutation = new();
            foreach (var chars in GetPermutation(Game1A2BAllChars, 4)!)
                charsAllPermutation.Add(new string(chars));
            if (countOfA == 0)
            {
                //此时只有B，我们是已知B的数量的
                //所以我们的得到所有B可能的位置情况
                List<int[]> indexCombinationOfAWhenA0 = GetCombination(indexs, countOfB)!;
                //遍历这些B的位置情况, 获得所有可能性, 然后合起来
                HashSet<string> allPermutation = new();
                for (int curB = 0; curB < indexCombinationOfAWhenA0.Count; curB++)
                {
                    var curBIndexArray = indexCombinationOfAWhenA0[curB];
                    foreach (var chars in charsAllPermutation)
                    {
                        bool needAdd = true;
                        for (int i = 0; i < curBIndexArray.Length; i++)
                        {
                            //确保指定数字不在指定位置
                            if (chars[curBIndexArray[i]] == nums[curBIndexArray[i]])
                            {
                                needAdd = false;
                                break;
                            }
                            //确保指定数字在这个排列里
                            bool isInChars = true;
                            for (int j = 0; j < curBIndexArray.Length; j++)
                            {
                                //这是那个指定的数字
                                var spNum = nums[curBIndexArray[j]];
                                //判断是否在排列里
                                if (!chars.Contains(spNum)) isInChars = false;
                            }
                            if (!isInChars)
                            {
                                needAdd = false;
                                break;
                            }
                        }
                        if (needAdd)
                            allPermutation.Add(chars);
                    }
                }
                return allPermutation;
            }
            if (countOfB == 0)
            {
                //此时只有A, 我们是已知A的数量的
                //所以我们得得到所有A可能的位置情况
                List<int[]> indexCombinationOfAWhenB0 = GetCombination(indexs, countOfA)!;
                //遍历这些A的位置情况, 获得所有可能性, 然后合起来
                HashSet<string> allPermutation = new();
                for (int curA = 0; curA < indexCombinationOfAWhenB0.Count; curA++)
                {
                    var curAIndexArray = indexCombinationOfAWhenB0[curA];
                    foreach (var chars in charsAllPermutation)
                    {
                        //chars是一种排列法, curAIndexArray是指目前假设位置正确的数字, index指向的是nums[]
                        //现在需要排除chars中 curAIndexArray 所在的位置的项不是同位置的nums[]里的项
                        bool needAdd = true;
                        for (int i = 0; i < curAIndexArray.Length; i++)
                        {
                            if (chars[curAIndexArray[i]] != nums[curAIndexArray[i]])
                            {
                                needAdd = false;
                                break;
                            }
                        }
                        if (needAdd) allPermutation.Add(chars);
                    }
                }
                //然后可以返回了
                return allPermutation;
            }


            HashSet<string> curGuessPermutation = new();
            List<int[]> indexCombinationOfA = GetCombination(indexs, countOfA)!;
            List<int[]> indexCombinationOfB = GetCombination(indexs, countOfB)!;
            for (int curA = 0; curA < indexCombinationOfA.Count; curA++)
            {
                for (int curB = 0; curB < indexCombinationOfB.Count; curB++)
                {
                    var curAIndexArray = indexCombinationOfA[curA];
                    var curBIndexArray = indexCombinationOfB[curB];
                    //不能允许假设的 A 和 B 假设了同一个index
                    if (!IsMutexWithEachOther(curAIndexArray, curBIndexArray))
                        continue;
                    //首先假设目前curA指定的index的位置的数字是正确的, 那么所有可能性
                    HashSet<string> curAPermutations = new();
                    foreach (var chars in charsAllPermutation)
                    {
                        //chars是一种排列法, curAIndexArray是指目前假设位置正确的数字, index指向的是nums[]
                        //现在需要排除chars中 curAIndexArray 所在的位置的项不是同位置的nums[]里的项
                        bool needAdd = true;
                        for (int i = 0; i < curAIndexArray.Length; i++)
                            if (chars[curAIndexArray[i]] != nums[curAIndexArray[i]]) needAdd = false;
                        if (needAdd) curAPermutations.Add(chars);
                    }

                    HashSet<string> curBPermutations = new();
                    foreach (var chars in charsAllPermutation)
                    {
                        bool needAdd = true;
                        for (int i = 0; i < curBIndexArray.Length; i++)
                        {
                            //确保指定数字不在指定位置
                            if (chars[curBIndexArray[i]] == nums[curBIndexArray[i]])
                            {
                                needAdd = false;
                                break;
                            }
                            //确保指定数字在这个排列里
                            bool isInChars = true;
                            for (int j = 0; j < curBIndexArray.Length; j++)
                            {
                                //这是那个指定的数字
                                var spNum = nums[curBIndexArray[j]];
                                //判断是否在排列里
                                if (!chars.Contains(spNum)) isInChars = false;
                            }
                            if (!isInChars)
                            {
                                needAdd = false;
                                break;
                            }
                        }
                        if (needAdd)
                            curBPermutations.Add(chars);
                    }

                    HashSet<string> allPermutations = new();
                    //取交集
                    curAPermutations.IntersectWith(curBPermutations);
                    allPermutations = curAPermutations;
                    curGuessPermutation.UnionWith(allPermutations);
                }
            }
            return curGuessPermutation;
        }

        /// <summary>
        /// 是否两个数组互斥(两个数组的元素没有交集)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrayA"></param>
        /// <param name="arrayB"></param>
        /// <returns></returns>
        public static bool IsMutexWithEachOther<T>(T[] arrayA, T[] arrayB)
        {
            foreach (var itemInA in arrayA) if (arrayB.Contains(itemInA)) return false;
            return true;
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