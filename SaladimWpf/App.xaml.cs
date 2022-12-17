using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions;
using Saladim.SalLogger;
using Microsoft.Extensions.DependencyInjection;
using SaladimQBot.Shared;
using SaladimQBot.Extensions;
using SaladimWpf.Services;

namespace SaladimWpf;

public partial class App : Application
{
    public new static App Current => Application.Current.Cast<App>();
    public IHost AppHost { get; protected set; }

    public App()
    {
        IServiceProvider serviceProvider = null!;
        SimCommandConfig simCommandConfig = new("/", typeof(App).Assembly, t => (CommandModule)serviceProvider.GetRequiredService(t));
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((c, coll) =>
            {
                coll.AddSalLoggerService(LogLevel.Trace);
                coll.AddSaladimWpf("ws://127.0.0.1:5000");
                coll.AddSimCommand(simCommandConfig);
                coll.AddSingleton<HttpRequesterService>();
                coll.AddSingleton<HttpServerService>();
                coll.AddSingleton<RandomService>();
                coll.AddSingleton<JavaScriptService>();
            })
            .Build();
        AppHost.Services.GetRequiredService<HttpServerService>();
        AppHost.RunAsync();
        serviceProvider = AppHost.Services;
        App.Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ss = AppHost.Services.GetRequiredService<SalLoggerService>();
        if (e.ExceptionObject is Exception exc)
            ss.SalIns.LogFatal("SaladimWpf", "AppDomainUnhandled", exc);
        ss.FlushFileStream();
    }

    private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        var ss = AppHost.Services.GetRequiredService<SalLoggerService>();
        ss.SalIns.LogFatal("SaladimWpf", "Dispatcher", e.Exception);
        ss.FlushFileStream();
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
