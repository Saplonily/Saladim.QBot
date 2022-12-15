using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp;
using SaladimQBot.SimCommand;

namespace SaladimSimCmd;

public class Program
{
    public static CqClient client = null!;
    public static SimCommandService simCmd = null!;

    public static async Task Main(string[] args)
    {
        client = new CqWebSocketClient("ws://127.0.0.1:5000", LogLevel.Trace);
        client.OnLog += Console.WriteLine;
        client.OnGroupMessageReceived += Client_OnGroupMessageReceived;
        simCmd = new(client, "");
        simCmd.AddModule(typeof(SampleModule));
        await client.StartAsync();
        Console.ReadLine();
        await client.StopAsync();
    }

    private static void Client_OnGroupMessageReceived(GroupMessage message, JoinedGroup group)
    {
        if (message.Group.GroupId == 860355679)
        {
            _ = simCmd.MatchAndExecuteAllAsync(message);
        }
    }
}

public class SampleModule : CommandModule
{
    [Command("random")]
    public void Random()
    {
        var builder = Content.Client.CreateMessageBuilder(Content.Message);
        builder.WithTextLine($"这是你的随机数~ {new Random().Next(0, 1000)}");
        _ = Content.MessageWindow.SendMessageAsync(builder.Build());
    }

    [Command("耗时方法")]
    public void TestMcd2()
    {
        Content.MessageWindow.SendMessageAsync("这个任务很耗时哦, 等会");
        Thread.Sleep(5000);
        Content.MessageWindow.SendMessageAsync("任务完成粒!");
    }

    [Command("echo")]
    public async void Echo(string contentToEcho)
    {
        await Content.MessageWindow.SendMessageAsync(contentToEcho);
    }
}