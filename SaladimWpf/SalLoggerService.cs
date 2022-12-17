using System.IO;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using Saladim.SalLogger;

namespace SaladimWpf;

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
            })
            .WithLevelLimit(logLevelLimit)
            .Build();
        hl.ApplicationStarted.Register(Start);
        hl.ApplicationStopped.Register(Stop);
    }

    ~SalLoggerService()
    {
        Stop();
    }

    public void Start()
    {
        DateTime now = DateTime.Now;
#if !DEBUG
        string path = @"Logs\";
#else
        string path = @"D:\User\Desktop\SaladimWPF\Logs\debug\";
#endif
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string unindexedFileName = $"{now.Year}.{now.Month}.{now.Day}";
        int index = 0;
        string filePath;
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
