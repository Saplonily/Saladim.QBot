using System.Reflection;

namespace EmptyTest;

internal class Program4
{
    static void Main(string[] args)
    {
        Type t = typeof(Hello);
        var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var field in fields)
        {
            Console.WriteLine($"{field.GetRawConstantValue()}");
        }
    }

    enum Hello
    {
        Hello1,
        Hello2 = 114514,
        Hello3 = 1919810
    }
}