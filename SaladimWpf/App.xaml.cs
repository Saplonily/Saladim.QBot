using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Saladim.SalLogger;
using SaladimQBot.Shared;
using SaladimWpf.Services;

namespace SaladimWpf;

public partial class App : Application
{
    public static readonly bool InDebug =
#if DEBUG
        true;
#else
        false;
#endif

    public new static App Current => Application.Current.Cast<App>();

    public IHost AppHost { get; protected set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((c, coll) =>
            {
                coll.AddSalLoggerService(LogLevel.Trace);
                coll.AddSaladimWpf("ws://127.0.0.1:5000");
            })
            .Build();
        Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ss = AppHost.Services.GetRequiredService<SalLoggerService>();
        if (e.ExceptionObject is Exception exc)
            ss.SalIns.LogFatal("SaladimWpf", "AppDomainUnhandled", exc);
        ss.FlushFileStream();
        ss.Stop();
    }

    private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        var ss = AppHost.Services.GetRequiredService<SalLoggerService>();
        ss.SalIns.LogFatal("SaladimWpf", "Dispatcher", e.Exception);
        ss.FlushFileStream();
        ss.Stop();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _ = AppHost.RunAsync();
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        await AppHost.StopAsync();
    }
}
