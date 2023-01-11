using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Hosting;
using Saladim.SalLogger;

namespace Saladim.Offbot.Services;

public class SalLoggerService
{
    protected StreamWriter streamWriter = null!;

    public Logger SalIns { get; protected set; } = null!;

    public event Action<string>? OnLog;

    public SalLoggerService(LogLevel logLevelLimit, IHostApplicationLifetime hl)
    {
        SalIns = new LoggerBuilder()
            .WithAction(s =>
            {
                OnLog?.Invoke(s);
                streamWriter.WriteLine(s);
                Debug.WriteLine(s);
            })
            .WithLevelLimit(logLevelLimit)
            .Build();
        this.Start();
        hl.ApplicationStopped.Register(Stop);
    }

    private void Start()
    {
        DateTime now = DateTime.Now;
        string path = @"Logs\";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string filePath = $@"{path}{now.Year}.{now.Month}.{now.Day}.log";
        bool fileExists = false;
        if (File.Exists(filePath))
            fileExists = true;
        streamWriter = new(filePath, true, Encoding.UTF8);
        if (fileExists)
            SalIns.LogRaw(LogLevel.Info, "\n\n\n");
        SalIns.LogInfo("LoggerService", $"Starting logging service at {DateTime.Now}.");
    }

    public void Stop()
    {
        streamWriter.Flush();
        streamWriter.Dispose();
    }

    public void FlushFileStream()
    {
        streamWriter.Flush();
    }
}
