using System.Text.Json;

string json = """
    {
        "pwp":1919810
    }
    """;
JsonDocument doc = JsonDocument.Parse(json);
doc.RootElement.TryGetProperty("awa", out var v);
Console.WriteLine(v.ValueKind);
Console.WriteLine(JsonValueKind.Undefined);


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