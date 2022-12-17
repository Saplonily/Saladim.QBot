using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public class SimCommandService
{
    public SimCommandExecutor Executor { get; protected set; }

    public SimCommandService(SimCommandConfig config)
    {
        var mi = config.ModuleInstancer;
        if (mi is null)
            Executor = new(config.RootPrefix);
        else
            Executor = new(config.RootPrefix, mi);
        foreach (var m in config.Modules)
        {
            Executor.AddModule(m);
        }
    }
}
