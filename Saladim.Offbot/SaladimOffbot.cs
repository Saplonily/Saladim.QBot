using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Saladim.Offbot.Services;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.Core.Services;

namespace Saladim.Offbot;

public class SaladimOffbot
{
    public IHost AppHost { get; protected set; }

    public SalLoggerService LoggerService { get; protected set; }

    public IClient Client { get; protected set; }

    public SaladimOffbot()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((c, coll) =>
            {
                coll.AddSalLoggerService(LogLevel.Trace);
                coll.AddSaladimWpf("ws://127.0.0.1:5000");
            })
            .Build();

        LoggerService = AppHost.Services.GetRequiredService<SalLoggerService>();
        LoggerService.OnLog += Console.WriteLine;
        Client = AppHost.Services.GetRequiredService<IClientService>().Client;
        AppDomain.CurrentDomain.ProcessExit += (obj, args) => OnProcessShutdown(null);
        Console.CancelKeyPress += (obj, args) => OnProcessShutdown(null);
        AppDomain.CurrentDomain.UnhandledException += (obj, args) => OnProcessShutdown(args.ExceptionObject is Exception e ? e : null);
    }

    public static async Task Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        SaladimOffbot saladimOffbot = new();
        Logger logger = saladimOffbot.LoggerService.SalIns;
        await saladimOffbot.RunAsync();
        string cmd;
        while (true)
        {
            cmd = Console.ReadLine()!.Trim();
            switch (cmd)
            {
                case "help": logger.LogInfo("Console", "help 帮助, exit 退出, stop 暂时停止, start 开始"); break;
                case "exit": goto end;
                case "stop": await saladimOffbot.Client.StopAsync().ConfigureAwait(false); break;
                case "start": await saladimOffbot.Client.StopAsync().ConfigureAwait(false); break;
            }
        }
    end:
        await saladimOffbot.StopAsync();
    }

    public async Task RunAsync()
    {
        await AppHost.Services.GetRequiredService<SaladimOffbotService>().StartAsync().ConfigureAwait(false);
        await AppHost.RunAsync().ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        await AppHost.Services.GetRequiredService<SaladimOffbotService>().StopAsync().ConfigureAwait(false);
        await AppHost.StopAsync().ConfigureAwait(false);

    }

    private void OnProcessShutdown(Exception? exception)
    {
        LoggerService.SalIns.LogInfo("Program", "Program is shutdowning...");
        if (exception is not null)
            LoggerService.SalIns.LogFatal("Program", exception, prefix: "Fatal exception occurred!");
        LoggerService.FlushFileStream();
        LoggerService.Stop();
    }
}
