using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmptyTest;

public class Program3
{
    static void Main(string[] args)
    {
        TestClass c = new()
        {
            str = "78958943",
            someInt = 1919810
        };
        var options = new JsonSerializerOptions()
        {
            Converters =
            {
                new TestConverter()
            }
        };
        var str = JsonSerializer.Serialize(c, options);
        Console.WriteLine(str);
    }
}

[JsonConverter(typeof(TestConverter))]
public class TestClass
{
    public string str { get; set; } = "114514";
    public int someInt { get; set; } = 114515;
}

public class TestConverter : JsonConverter<object>
{
    public static int IDMAX = 0;
    public int ID = 0;
    public TestConverter()
    {
        Console.WriteLine("ctor called");
        ID = IDMAX++;
    }
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Console.WriteLine("READ");
        return null;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        Console.WriteLine("WRITE");
        writer.WriteString("WRITE in writer", "In value");
    }

    public override bool CanConvert(Type typeToConvert)
    {
        Console.WriteLine($"{ID} -> tried: {typeToConvert}");
        Console.WriteLine("t for TRUE,f for FALSE");
        var k = Console.ReadKey();
        if (k.KeyChar == 't')
            return true;
        Console.WriteLine();
        return false;
    }
}