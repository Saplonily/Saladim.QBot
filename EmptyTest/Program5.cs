Person p = new();
var a = p.Name = "114514";

public class Person
{
    public string Name
    {
        get
        {
            Console.WriteLine("get执行");
            return "114514";
        }
        set
        {
            Console.WriteLine($"set执行,值是{value}");
        }
    }
}



/*for(float f = 0.0f;f<=10.0f;f+=0.1f)
{
    Console.WriteLine($"{f}-{float.IsNormal(f)}");
}
Console.WriteLine(float.IsNormal(float.PositiveInfinity));
Console.WriteLine(float.IsNormal(float.NegativeInfinity));
Console.WriteLine(float.IsNormal(float.NaN));
Console.WriteLine(float.IsNormal(float.MaxValue));
Console.WriteLine(float.IsNormal(0.0f));
*/
/*
using System.Text.Json;
using System.Text.Json.Serialization;

string json = """
    {
        "qwq":114514,
        "T114514":1919810
    }
    """;

AnotherRecord a = new()
{
    T114514 = 1919810,
    qwq = 23333
};
Console.WriteLine(a);
Console.WriteLine(JsonSerializer.Serialize(a));

public abstract record SomeRecord()
{
    [JsonIgnore]
    public abstract int qwq { get; set; }
}
public record AnotherRecord() : SomeRecord()
{
    public int T114514 { get; set; }

    public override int qwq { get => 11111111; set { } }
}*/


/*
using System.Text.Json;

string json = """
    {
        "pwp":1919810
    }
    """;
JsonDocument doc = JsonDocument.Parse(json);
doc.RootElement.TryGetProperty("awa", out var v);
Console.WriteLine(v.ValueKind);
Console.WriteLine(JsonValueKind.Undefined);*/


/*using System.Text.Json;

string json = """
    {
        "awa":null,
        "pwp":1919810
    }
    """;
JsonDocument doc = JsonDocument.Parse(json);
Console.WriteLine(doc.RootElement.GetProperty("awa").GetString());
*/