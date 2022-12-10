using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Saladim.SalLogger;

namespace SaladimWpf;

public partial class App : Application
{
    public TextBox? TextBoxLogging;
    public CheckBox? ScrollToEndCheckBox;
    public Logger Logger;
    public StreamWriter streamWriter;

    public App()
    {
        DateTime now = DateTime.Now;
        string path = @"Logs\";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string unindexedFileName = $"{now.Year}.{now.Month}.{now.Day}";
        string filePath = string.Empty;
        int index = 0;
        do
        {
            string indexedFileName = $"{unindexedFileName} {index}.log";
            string combinedPath = Path.Combine(path, indexedFileName);
            if (!File.Exists(combinedPath))
            {
                filePath = combinedPath;
                break;
            }
            else
            {
                index++;
                continue;
            }
        }
        while (true);

        streamWriter = new(filePath);
        Logger = new LoggerBuilder()
            .WithAction(s =>
            {
                Current.Dispatcher.Invoke(() =>
                {
                    TextBoxLogging?.AppendText(s + Environment.NewLine);
                    if (ScrollToEndCheckBox?.IsChecked is true)
                    {
                        TextBoxLogging?.ScrollToEnd();
                    }
                });
                streamWriter.WriteLine(s);
            })
            .WithLevelLimit(LogLevel.Trace)
            .Build();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        DispatcherUnhandledException += this.App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.ProcessExit += this.CurrentDomain_ProcessExit;
        AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            if (e.ExceptionObject is Exception ex)
                Logger.LogFatal("WpfConsole", "CurrentDomainUnhandled", ex);
            streamWriter.Dispose();
        }
        catch { };
    }

    private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
    {
        try
        {
            streamWriter.Dispose();
        }
        catch { };
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            Logger.LogFatal("WpfConsole", "Domain", e.Exception);
            streamWriter.Dispose();
            e.Handled = true;
            this.Shutdown();
        }
        catch { }
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        try
        {
            streamWriter.Dispose();
        }
        catch { };
    }
}
