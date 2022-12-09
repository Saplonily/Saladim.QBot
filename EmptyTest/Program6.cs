
Person p1 = new("第一个人", 20);
Person p2 = new("第一个人", 15);

public class Person : ICloneable
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Person(string name, int age) => (Name, Age) = (name, age);
}






/*Console.WriteLine("nothing");

public class A : ValueType
{
    public A()
    {
    }
}*/

/*var type = typeof(Warp);
var prop = type.GetProperty("SomeThing")!.PropertyType;
Console.WriteLine(prop.GetGenericTypeDefinition());
Console.WriteLine(type.Namespace);

public class Warp
{
    public Nullable<int> SomeThing { get; set; }
}*/

/*Console.Title = "test";
Thread th = new(() =>
{
    Thread.CurrentThread.Name = "Another working thread.";
    while (true)
    {
        Thread.Sleep(500);
        Console.WriteLine("另一个线程");
    }
});
th.Start();

while(true)
{
    Thread.Sleep(500);
    Console.WriteLine("Main线程");
}*/

/*
public class Program6
{
    static void Main(string[] args)
    {
        var warpedIntType = typeof(Warp<int>);
        var field = warpedIntType.GetField("Value");
        Console.WriteLine(field.FieldType);
    }
}

public class Warp<T>
{
    public T? Value;
}*/