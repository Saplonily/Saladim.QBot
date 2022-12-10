using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;

namespace DocExample;

internal class Program
{
    static async Task Main(string[] args)
    {
        CqClient client = new CqWebSocketClient("ws://127.0.0.1:5000", LogLevel.Trace);
        client.OnMessageReceived += Client_OnMessageReceived;
        client.OnLog += Console.WriteLine;

        await client.StartAsync();

        Console.ReadLine();

        await client.StopAsync();
    }

    private static void Client_OnMessageReceived(Message message)
    {
        var chain = message.MessageEntity.Chain;
        var allAtNode = chain.AllAt();
        foreach(var atNode in allAtNode)
        {
            Console.WriteLine($"{atNode.User.Nickname.Value}被@了");
        }
    }
}
