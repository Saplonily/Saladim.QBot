using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Saladim.Offbot.Entity;
using Saladim.Offbot.Services;
using Saladim.SalLogger;
using SaladimQBot.Extensions;
using SqlSugar;

namespace Saladim.Offbot;

public static class ServicesExtensions
{
    public static void AddSaladimWpf(this IServiceCollection services, string goCqHttpWebSocketAddress)
    {
        services.AddSingleton(_ => new SaladimOffbotServiceConfig(goCqHttpWebSocketAddress));
        services.AddHostedService<SaladimOffbotService>();

        services.AddSimCommand(s => new(t => (CommandModule)s.GetRequiredService(t), "/", "!", "s>"), typeof(Program).Assembly);

        services.AddSingleton<CoroutineService>();
        services.AddSingleton<HttpRequesterService>();
        services.AddSingleton<RandomService>();

        services.AddSingleton(s =>
        {
            string connectionString =
#if DEBUG
            @"DataSource=D:\User\Desktop\Saladim.Offbot\data\debug.db";
#else
            @"DataSource=D:\User\Desktop\Saladim.Offbot\data\release.db";
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
            scope.CodeFirst.InitTables(typeof(Program).Assembly.GetTypes().Where(t => t.Namespace == "Saladim.Offbot.Entity").ToArray());
            return scope;
        });


        services.AddSingleton<MemorySessionService>();

        services.AddSingleton<IntegralCalculatorService>();
        services.AddSingleton<Auto1A2BService>();
        services.AddSingleton<HomoService>();
        services.AddSingleton<SdSysService>();

        services.AddSingleton<FiveInARowService>();
    }

    public static void AddSalLoggerService(this IServiceCollection services, LogLevel logLevel)
    {
        services.TryAddSingleton<SalLoggerService>(s => new(logLevel, s.GetRequiredService<IHostApplicationLifetime>()));
    }
}
