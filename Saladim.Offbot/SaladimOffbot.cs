using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Saladim.Offbot.Services;
using Saladim.SalLogger;
using SaladimQBot.Core;
using SaladimQBot.Core.Services;
using SaladimQBot.GoCqHttp;

namespace Saladim.Offbot;

public static class SaladimOffbot
{
    private static Logger logger = null!;

    private static SalLoggerService loggerService = null!;

    public static async Task Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((c, coll) =>
            {
                coll.AddSalLoggerService(LogLevel.Trace);
                coll.AddSaladimWpf("ws://127.0.0.1:5000");
            })
            .Build();

        AppDomain.CurrentDomain.ProcessExit += (obj, args) => OnProcessShutdown(null);
        Console.CancelKeyPress += (obj, args) => OnProcessShutdown(null);
        AppDomain.CurrentDomain.UnhandledException += (obj, args) => OnProcessShutdown(args.ExceptionObject is Exception e ? e : null);

        loggerService = host.Services.GetRequiredService<SalLoggerService>();
        logger = loggerService.SalIns;
        await host.RunAsync().ContinueWith(t =>
        {
            logger.LogInfo("Offbot", "RunAsync task stopped.");
            if (t.Exception is not null)
            {
                logger.LogError("Offbot", t.Exception, "Error when running the host");
            }
        });

        await host.StopAsync().ContinueWith(t =>
        {
            logger.LogInfo("Offbot", "StopAsync task stopped.");
            if (t.Exception is not null)
            {
                logger.LogError("Offbot", t.Exception, "Error when stopping the host.");
            }
        });

        static void OnProcessShutdown(Exception? exception)
        {
            logger.LogInfo("Program", "Program is shutdowning...");
            if (exception is not null)
                logger.LogFatal("Program", exception, prefix: "Fatal exception occurred!");
            loggerService.FlushFileStream();
            loggerService.Stop();
        }
    }
}
