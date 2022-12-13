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

        await client.StartAsync().ConfigureAwait(false);

        Console.ReadLine();

        await client.StopAsync().ConfigureAwait(false);
    }

    private static async void Client_OnMessageReceived(Message message)
    {
        string rawString = message.MessageEntity.RawString.Trim();
        string command = "/echo ";
        if (rawString.StartsWith(command))
        {
            await message.MessageWindow.SendMessageAsync(rawString.Substring(command.Length)).ConfigureAwait(false);
        }
        /*
        var chain = message.MessageEntity.Chain;
        var allImageNode = chain.AllImage();
        await message.MessageWindow.SendMessageAsync($"一条消息被发送出来了, 它包含{allImageNode.Count()}个图片节点.").ConfigureAwait(false);*/
    }
}
