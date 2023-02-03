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
        services.AddHostedService<SaladimOffbotService>();

        services.AddSimCommand(s => new("/", t => (CommandModule)s.GetRequiredService(t)), typeof(Program).Assembly);

        services.AddSingleton<CoroutineService>();
        services.AddSingleton<HttpRequesterService>();
        services.AddSingleton<RandomService>();

        services.AddSingleton<SqlSugarScope>(s =>
        {
            string connectionString =
#if DEBUG
            @"DataSource=D:\User\Desktop\SaladimWPF\data\debug.db";
#else
            @"DataSource=D:\User\Desktop\SaladimWPF\data\release.db";
#endif
            var loggerService = s.GetRequiredService<SalLoggerService>();
            var scope = new SqlSugarScope(new ConnectionConfig()
            {
                DbType = DbType.Sqlite,
                ConnectionString = connectionString,
                IsAutoCloseConnection = true,
            });
            scope.Aop.OnLogExecuting = (sql, args) =>
            {
                loggerService.SalIns.LogInfo("Offbot", "SqlExecuting", UtilMethods.GetSqlString(DbType.SqlServer, sql, args));
            };
            return scope;
        });
        services.AddSingleton<SessionSugarStoreService>();
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
