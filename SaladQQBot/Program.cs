namespace SaladQQBot;

public class Program
{
    public static void Main(string[] args)
    {
        new Program().MainAsync(args).Wait();
    }

    public async Task MainAsync(string[] args)
    {
        Logger.OnLog += Console.WriteLine;

        Directory.SetCurrentDirectory("H:\\Projects\\QBotDotnet\\Working");

        Client client = new();

        client.OnMessageArrived += (msg) =>
        {
            Logger.LogInfo($"[User] {msg.Sender.Name}: {msg.RawContent} {msg.Sender.Level}");
            //msg.Sender.SendMessageAsync(new MessageBuilder().AddText("你好").Build());
        };

        bool success = false;
        while (true)
        {
            success = await client.StartAsync();
            if (success) break;
            Logger.LogInfo("启动客户端失败... 2s后自动重连...");
            Thread.Sleep(2000);
        }

        Console.ReadLine();

        client.StopAsync();
        Console.Read();
    }
}