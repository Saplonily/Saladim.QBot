using SaladimWpf.Services;

/*using IlyfairyLib.Unsafe;

var someInterface = UnsafeHelper.AllocObject<ISomeInterface>();
var members = someInterface.GetType().GetMembers();
foreach (var m in members)
{
    Console.WriteLine(m);
}

public interface ISomeInterface
{
    void SomeMethod();
}

public class SomeClass : ISomeInterface
{
    public void SomeMethod()
    {
        Console.WriteLine("somemethodoutput");
    }
}*/


/*const int exTimes = 1000000;

Random r = new(DateTime.Now.Second + (int)DateTime.Now.Ticks);
for (int count = 1; ; count += 1)
{
    bool[] bools = new bool[count];
    int[] conflictTimes = new int[exTimes];
    for (int p = 0; p < exTimes; p++)
    {
        bools.AsSpan().Fill(false);
        for (int i = 1; i < count + 2; i++)
        {
            if (ARandomize(bools))
            {
                conflictTimes[p] = i;
                break;
            }
        }
    }
    GC.Collect();
    Console.WriteLine($"数量{count}平均次数: {conflictTimes.Average():F3}");
}

bool ARandomize(bool[] bools)
{
    int index = r.Next(bools.Length);
    bool rt = bools[index];
    bools[index] = true;
    return rt;
}*/

/*AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

void CurrentDomain_ProcessExit(object? sender, EventArgs e)
{
    File.WriteAllText("114514.txt", "114514");
}*/




/*using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("1");
        var attrs = ((Action<string[]>)Foo).Method.GetParameters()[0].GetCustomAttributes();
        Console.WriteLine(attrs);
    }

    private static void Foo(params string[] strs)
    {
        Console.WriteLine(strs);
    }
}*/

/*using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, s) =>
    {
        s.AddHostedService<SomeService>();
        s.AddLogging(er =>
        {
            er.Add
        })
    })
    .Build();
_ = Task.Run(() =>
{
    Thread.Sleep(5000);
    host.StopAsync();
});
Console.WriteLine(host);
var theService = host.Services.GetRequiredService(typeof(SomeService));
Console.WriteLine(theService ?? "is null...");
await host.RunAsync();

public class SomeService : BackgroundService
{
    protected Timer timer = null!;

    public SomeService()
    {
        Console.WriteLine("built someService...");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        timer = new(_ =>
        {
            Console.WriteLine("Working...");
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        return Task.CompletedTask;
    }
}*/


/*Console.WriteLine("你好f34f34f34f3qwq".AsSpan().ToString());*/

/*Delegate a = () => Console.WriteLine("114514");
a.DynamicInvoke();*/


/*#nullable disable

using static ISenderFactory;

internal class Program
{


    private static void Main(string[] args)
    {
        Console.WriteLine();
        SenderFactory f = new();
        f.OnMakeSender += F_OnMakeSender;
    }

    private static void F_OnMakeSender(ISender sender)
    {
        Console.WriteLine(sender.Name);
    }

    private static void SomeProcess(ISender sender)
    {
        Console.WriteLine($"Process: {sender.Name}");
        
    }
}

interface ISenderFactory
{
    public delegate void FactoryEventHandler<in T>(T arg);
    //public delegate void OnMakeISenderHandler(ISender sender);
    public event FactoryEventHandler<ISender> OnMakeISender;
    ISender Make();
}

class SenderFactory : ISenderFactory
{
    public delegate void OnMakeSenderHandler(Sender sender);
    public event FactoryEventHandler<Sender> OnMakeSender;
    public Sender Make() => new();
    ISender ISenderFactory.Make() => Make();

    event ISenderFactory.FactoryEventHandler<ISender> ISenderFactory.OnMakeISender { add => OnMakeSender += value; remove => OnMakeSender -= value; }
}

interface ISender { string Name { get; } }
class Sender : ISender { public string Name { get; } = "Test"; }
*/

/*SomeClass s = new();
s.Prop = 1;
Console.WriteLine(s.Prop);

public class SomeClass
{
    private int prop = 0;

    public int Prop
    {
        get => prop;
        set => prop = value;
    }
}*/

/*NormalStruct n = new(2);
ISayable u = n;
ISayable u2 = n;
n.Value = 4;
u.Say();
n.Value = 9;
u2.Say();
MakeSay(u);

void MakeSay(ISayable sayable)
{
    sayable.Say();
}
public class NormalClass : ISayable
{
    public int Value;
    public NormalClass(int value) => Value = value;
    public void Say()
    {
        Console.WriteLine($"I'm NormalClass: {Value}");
    }
}
public struct NormalStruct : ISayable
{
    public int Value;
    public NormalStruct(int value) => Value = value;
    public void Say()
    {
        Console.WriteLine($"I'm NormalStruct: {Value}");
    }
}
public interface ISayable
{
    void Say();
}*/




/*A a = new();
public class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}*/




/*Console.WriteLine("Hello World.");
Console.ReadLine();*/

/*using System.Net.Http.Headers;
HttpClient client = new();
string json = """
    {
        "group_id": 860355679
    }
    """;
StringContent content = new(json);
content.Headers.Clear();
content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
content.Headers.ContentEncoding.Add("utf-8");
//content.Headers.Add("Content-Type", "application/json; charset=utf-8");
var res = await client.PostAsync("http://127.0.0.1:5700/get_group_info", content);
string s = await res.Content.ReadAsStringAsync();
Console.WriteLine(s);*/


/*using System.Net;
using System.Text;

HttpListener listener = new();
listener.Prefixes.Add("http://127.0.0.1:5566/");
listener.Start();

while (true)
{
    var context = listener.GetContext();

    var method = context.Request.HttpMethod;

    HttpListenerResponse response = context.Response;
    response.StatusCode = (int)HttpStatusCode.OK;
    response.ContentType = "application/json;charset=UTF-8";
    response.ContentEncoding = Encoding.UTF8;
    response.AppendHeader("Content-Type", "application/html;charset=UTF-8");

    using StreamWriter writer = new(response.OutputStream);
    writer.WriteLine($"这是回应, 你使用的方法是{method}");
    string body = new StreamReader(context.Request.InputStream).ReadToEnd();
    writer.WriteLine($"你的请求body是:\n{body}");

    Console.WriteLine($"请求来了, url是{context.Request.Url}, body是\n{body}\n------");
}*/

/*{
    Something();
}

{
    GC.Collect();
}
void Something()
{ 
    DisposeableThing thing = new();
}

public class DisposeableThing : IDisposable
{
    public void Dispose()
    {
        Console.WriteLine("disposed");
        GC.SuppressFinalize(this);
    }

    ~DisposeableThing()
    {
        Console.WriteLine("析构");
    }
}*/



/*public class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello world");
        Person p = new("114514", 20);
        Person p2 = new("114514", 20);
        Console.WriteLine(p == p2);
    }
}

public class Person
{
    public Person(string name, int age)
    {
        this.Name = name;
        this.Age = age;
    }

    public string Name { get; set; }

    public int Age { get; set; }

    public override bool Equals(object? obj)
    {
        bool r = obj is Person person &&
               this.Name == person.Name &&
               this.Age == person.Age;
        Console.WriteLine("进行了一次Equals比较");
        return r;
    }

    public override int GetHashCode()
    {
        Console.WriteLine("进行了一次GetHashCode");
        return HashCode.Combine(this.Name, this.Age);
    }

    public static bool operator ==(Person? left, Person? right)
    {
        Console.WriteLine("进行了一次==");
        return EqualityComparer<Person>.Default.Equals(left, right);
    }

    public static bool operator !=(Person? left, Person? right)
    {
        Console.WriteLine("进行了一次!=");
        return !(left == right);
    }
}*/