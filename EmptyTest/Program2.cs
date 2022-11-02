namespace EmptyTest;

public class Program2
{
    static async Task Main2(string[] args)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.

        Dictionary<string, string> values = new()
        {
            ["from"] = "zh",
            ["to"] = "en",
            ["token"] = "53c63698f932d02f080c30eeafb8990f",
            ["query"] = "你好啊",
            ["domain"] = "common",
            ["transtype"] = "translang",
            ["simple_meas_flag"] = "3"
        };

        // 数据转化为 key=val 格式
        var content = new FormUrlEncodedContent(values);
        

        // 发送请求
        var response = await client.PostAsync("https://fanyi.baidu.com/v2transapi?from=zh&to=en", content);
        // 获取数据
        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseString);
    }
}