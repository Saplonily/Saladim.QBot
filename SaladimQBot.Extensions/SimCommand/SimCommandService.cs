using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SaladimQBot.Extensions;

public class SimCommandService
{
    public SimCommandExecuter Executer { get; protected set; }

    public SimCommandService(SimCommandConfig config)
    {
        var mi = config.ModuleInstancer;
        if (mi is null)
            Executer = new(config.RootPrefixes);
        else
            Executer = new(mi, config.RootPrefixes);
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
                service.Executer.AddModule(module);
            return service;
        });
        services.AddSingleton(s => configProvider(s));
        foreach (var module in toBeAddModules)
        {
            services.AddTransient(module);
        }
    }
}