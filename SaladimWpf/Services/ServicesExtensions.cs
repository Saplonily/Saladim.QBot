using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Saladim.SalLogger;
using SaladimQBot.Core.Services;
using SaladimQBot.Extensions;
using SqlSugar;

namespace SaladimWpf.Services;

public static class ServicesExtensions
{
    public static void AddSaladimWpf(this IServiceCollection services, string goCqHttpWebSocketAddress)
    {
        services.AddSingleton(_ => new SaladimWpfServiceConfig(goCqHttpWebSocketAddress));
        services.AddSingleton<SaladimWpfService>();
        services.AddSingleton<IClientService, SaladimWpfService>(s => s.GetRequiredService<SaladimWpfService>());

        services.AddSimCommand(s => new("/", t => (CommandModule)s.GetRequiredService(t)), typeof(App).Assembly);

        string connectionString = App.InDebug ?
                    @"DataSource=D:\User\Desktop\SaladimWPF\data\debug.db" :
                    @"DataSource=D:\User\Desktop\SaladimWPF\data\release.db";
        services.AddSingleton(s => new ConnectionConfig()
        {
            DbType = DbType.Sqlite,
            ConnectionString = connectionString,
            IsAutoCloseConnection = true
        });

        services.AddSingleton<CoroutineService>();
        services.AddSingleton<HttpRequesterService>();
        services.AddSingleton<RandomService>();

        services.AddSingleton<SessionSqliteService>();
        services.AddSingleton<MemorySessionService>();

        services.AddSingleton<JavaScriptService>();
        services.AddSingleton<IntegralCalculatorService>();
        services.AddSingleton<Auto1A2BService>();

        services.AddSingleton<FiveInARowService>();
    }

    public static void AddSimCommand(this IServiceCollection services, Func<IServiceProvider, SimCommandConfig> configProvider, Assembly modulesAsm)
    {
        var toBeAddModules = modulesAsm.GetTypes().Where(t => t.IsSubclassOf(typeof(CommandModule)));
        services.AddSingleton<SimCommandService>(s =>
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

    public static void AddSalLoggerService(this IServiceCollection services, LogLevel logLevel)
    {
        services.AddSingleton<SalLoggerService>(s => new(logLevel, s.GetRequiredService<IHostApplicationLifetime>()));
    }
}
