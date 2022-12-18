using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Saladim.SalLogger;
using SaladimQBot.Core.Services;
using SaladimQBot.Extensions;

namespace SaladimWpf.Services;

public static class ServicesExtensions
{
    public static void AddSaladimWpf(this IServiceCollection services, string goCqHttpWebSocketAddress)
    {
        services.AddSingleton(_ => new SaladimWpfServiceConfig(goCqHttpWebSocketAddress));
        services.AddSingleton<SaladimWpfService>();
        services.AddSingleton<IClientService, SaladimWpfService>();
    }

    public static void AddSimCommand(this IServiceCollection services, SimCommandConfig config)
    {
        services.AddSingleton<SimCommandService>();
        services.AddSingleton(config);
        foreach (var module in config.Modules)
        {
            services.AddTransient(module);
        }
    }

    public static void AddSalLoggerService(this IServiceCollection services, LogLevel logLevel)
    {
        services.AddSingleton<SalLoggerService>(s => new(logLevel, s.GetRequiredService<IHostApplicationLifetime>()));
    }
}
