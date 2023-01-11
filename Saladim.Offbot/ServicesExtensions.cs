using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Saladim.Offbot.Services;
using Saladim.SalLogger;
using SaladimQBot.Core.Services;
using SaladimQBot.Extensions;
using SqlSugar;

namespace Saladim.Offbot;

public static class ServicesExtensions
{
    public static void AddSaladimWpf(this IServiceCollection services, string goCqHttpWebSocketAddress)
    {
        services.AddSingleton(_ => new SaladimOffbotServiceConfig(goCqHttpWebSocketAddress));
        services.AddSingleton<SaladimOffbotService>();
        services.AddSingleton<IClientService, SaladimOffbotService>(s => s.GetRequiredService<SaladimOffbotService>());

        services.AddSimCommand(s => new("/", t => (CommandModule)s.GetRequiredService(t)), typeof(SaladimOffbot).Assembly);

        string connectionString =
#if DEBUG
            @"DataSource=D:\User\Desktop\SaladimWPF\data\debug.db";
#else
            @"DataSource=D:\User\Desktop\SaladimWPF\data\release.db";
#endif
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

        services.AddSingleton<IntegralCalculatorService>();
        services.AddSingleton<Auto1A2BService>();
        services.AddSingleton<HomoService>();

        services.AddSingleton<FiveInARowService>();
    }

    public static void AddSalLoggerService(this IServiceCollection services, LogLevel logLevel)
    {
        services.TryAddSingleton<SalLoggerService>(s => new(logLevel, s.GetRequiredService<IHostApplicationLifetime>()));
    }
}
