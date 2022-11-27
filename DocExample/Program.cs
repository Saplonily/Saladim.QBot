using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;

namespace DocExample;

internal class Program
{
    static async Task Main(string[] args)
    {
        CqClient client = new CqWebSocketClient("ws://127.0.0.1:8080", LogLevel.Trace);
        client.OnMessageReceived += Client_OnMessageReceived;
        client.OnLog += Console.WriteLine;

        await client.StartAsync();

        Console.ReadLine();

        await client.StopAsync();
    }

    private static void Client_OnMessageReceived(Message message)
    {
        string rawString = message.MessageEntity.RawString;
        long userId = message.Sender.UserId;
        string nickName = message.Sender.Nickname.Value;
        Console.WriteLine($"{nickName}({userId})说: {rawString}");

        string echoCommandPrefix = "/echo ";
        string msgTrimed = rawString.Trim();
        if (msgTrimed.StartsWith(echoCommandPrefix))
        {
            string messageToSend = msgTrimed[echoCommandPrefix.Length..];
            var result = message.MessageWindow.SendMessageAsync(messageToSend).Result;
            Console.WriteLine($"消息已发送({result.MessageId}): {messageToSend}");
        }
    }
}
