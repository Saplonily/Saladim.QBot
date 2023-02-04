using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SaladimQBot.Extensions;

public class SimCommandService
{
    public SimCommandExecuter Executor { get; protected set; }

    public SimCommandService(SimCommandConfig config)
    {
        var mi = config.ModuleInstancer;
        if (mi is null)
            Executor = new(config.RootPrefix);
        else
            Executor = new(config.RootPrefix, mi);
    }
}

public static partial class ServiceExtensions
{
    public static void AddSimCommand(this IServiceCollection services, Func<IServiceProvider, SimCommandConfig> configProvider, Assembly modulesAsm)
    {
        var toBeAddModules = modulesAsm.GetTypes().Where(t => t.IsSubclassOf(typeof(CommandModule)));
        services.AddSingleton(s =>
        {
            SimCommandService service = new(s.GetRequiredService<SimCommandConfig>());
            foreach (var module in toBeAddModules)
                service.Executor.AddModule(module);
            return service;
        });
        services.AddSingleton(s => configProvider(s));
        foreach (var module in toBeAddModules)
        {
            services.AddTransient(module);
        }
    }
}