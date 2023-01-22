using SaladimQBot.Core;
using System.Diagnostics;
using System.Text;
using SaladimQBot.Extensions;
using SaladimQBot.GoCqHttp;

namespace Saladim.Offbot.SimCmdModules;

public class InfoModule : CommandModule
{
    public InfoModule()
    {

    }

    [Command("check_env")]
    public void CheckEnv()
    {
        var os = Environment.OSVersion;
        StringBuilder sb = new();
        sb.AppendLine($"运行平台: {os.Platform}");
#if DEBUG
        sb.AppendLine($"环境: DEBUG");
#else
        sb.AppendLine($"环境: RELEASE");
#endif
        sb.AppendLine($"运行程序: {Process.GetCurrentProcess().ProcessName}");
        sb.AppendLine($".NET clr版本: {Environment.Version}");
        sb.AppendLine($"程序内存占用: {NumberHelper.GetSizeString(Process.GetCurrentProcess().PagedMemorySize64)}");
        Content.MessageWindow.SendMessageAsync(sb.ToString());
    }

    [Command("gc_collect")]
    public void GCCollect()
    {
        Console.Clear();
        GC.Collect(0);
        GC.Collect(1);
        GC.Collect(2);
        Content.MessageWindow.SendTextMessageAsync($"已完成0,1,2三代清理工作");
    }
}
