using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Saladim.Offbot.Services;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.Core.Services;
using SaladimQBot.GoCqHttp;

namespace Saladim.Offbot;

public class SaladimOffbot
{
    protected Logger logger;

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
        Client.OnStoppedUnexpectedly += this.Client_OnStoppedUnexpectedly;
        AppDomain.CurrentDomain.ProcessExit += (obj, args) => OnProcessShutdown(null);
        Console.CancelKeyPress += (obj, args) => OnProcessShutdown(null);
        AppDomain.CurrentDomain.UnhandledException += (obj, args) => OnProcessShutdown(args.ExceptionObject is Exception e ? e : null);
    }

    private void Client_OnStoppedUnexpectedly(Exception ce)
    {
        Task.Run(async () =>
        {
            LoggerService.SalIns.LogError("Offbot", ce, $"Client session stopped unexpectetedly! " +
                $"Next connection try will be started in 2s.");
            Thread.Sleep(2000);
            try
            {
                await Client.StartAsync();
            }
            catch (ClientException e)
            {
                logger.LogError("Offbot", e, "Error when trying to reconnect the saladimOffbot service");
            }
        });
    }

    public static async Task Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        SaladimOffbot saladimOffbot = new();
        Logger salLoggerIns = saladimOffbot.LoggerService.SalIns;
        saladimOffbot.logger = salLoggerIns;
        var logger = saladimOffbot.logger;
        _ = saladimOffbot.RunAsync().ContinueWith(t =>
        {
            logger.LogInfo("Offbot", "RunAsync task stopped.");
            if (t.Exception is not null)
            {
                logger.LogError("Offbot", t.Exception, "Error when running the saladimOffbot service");
            }
        });
        string cmd;
        while (true)
        {
            cmd = Console.ReadLine()!.Trim();
            switch (cmd)
            {
                case "help": logger.LogInfo("Console", "help 帮助, exit 退出, stop 暂时停止, start 开始"); break;
                case "exit": goto end;
                case "stop": await saladimOffbot.Client.StopAsync().ConfigureAwait(false); break;
                case "start": await saladimOffbot.Client.StartAsync().ConfigureAwait(false); break;
            }
        }
    end:
        try
        {
            await saladimOffbot.StopAsync();
        }
        catch (Exception e)
        {
            logger.LogWarn("Offbot", e, "Error when try stopping service.");
        }
    }

    public async Task RunAsync()
    {
        await AppHost.Services.GetRequiredService<SaladimOffbotService>().StartAsync();
        await AppHost.RunAsync();
    }

    public async Task StopAsync()
    {
        await AppHost.StopAsync();
        await AppHost.Services.GetRequiredService<SaladimOffbotService>().StopAsync();
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
