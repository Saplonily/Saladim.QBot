using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;

namespace DocExample;

internal class Program
{
    static async Task Main(string[] args)
    {
        CqClient client = new CqWebSocketClient("ws://127.0.0.1:8081", LogLevel.Trace);
        client.OnClientEventOccurred += Client_OnClientEventOccurred;

        client.OnLog += Console.WriteLine;

        await client.StartAsync();

        Console.ReadLine();

        await client.StopAsync();
    }

    private static void Client_OnClientEventOccurred(ClientEvent clientEvent)
    {
        if (clientEvent is ClientMessageReceivedEvent clientMessageReceivedEvent)
        {
            Client_OnMessageReceived(clientMessageReceivedEvent.Message);
        }
    }

    private static async void Client_OnMessageReceived(Message message)
    {
        string rawString = message.MessageEntity.RawString.Trim();

        var chain = message.MessageEntity.Chain;
        await Console.Out.WriteLineAsync(new MessageEntity(message.Client, chain).RawString);
    }
}
