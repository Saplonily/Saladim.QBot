using System.Text.Json;
using System.Text.Json.Serialization;
/*
    屑sll用来实验用的



*/
namespace Test;

public class Program
{
    public static void Main(string[] args)
    {
        string json = """
            {
                "SomeString":"1919810",
                "Hello motherfucker~":1919810,
                "Happy":"yes",
                "Something":[
                    "1",
                    "2",
                    "3"
                ]
            }
            """;
        var m = JsonSerializer.Deserialize<MySbData>(json);
        Console.WriteLine(m.SomeString);
    }
}

public class MySbConverter : JsonConverter<MySbData>
{
    public override MySbData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        MySbData sb = new();
        while (reader.Read())
        {
            sb.SomeString += reader.TokenType switch
            {
                JsonTokenType.PropertyName => "PropertyName",
                JsonTokenType.String => "String",
                JsonTokenType.Number => "Number",
                _ => "--" + reader.TokenType + "--",
            };
        }
        return sb;
    }

    public override void Write(Utf8JsonWriter writer, MySbData value, JsonSerializerOptions options)
    {

    }
}

[JsonConverter(typeof(MySbConverter))]
public class MySbData
{
    [JsonInclude]
    public string SomeString = "114514";
}