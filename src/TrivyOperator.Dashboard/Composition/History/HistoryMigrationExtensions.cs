using TrivyOperator.Dashboard.Infrastructure.History.Migrations;
using TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator;
using TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator.Abstractions;

namespace TrivyOperator.Dashboard.Composition.History;

public static class HistoryMigrationExtensions
{
    internal static void AddHistoryMigrationServices(this IServiceCollection services)
    {
        services.AddSingleton<IPersistenceMigrationHistoryStore, PersistenceMigrationHistoryStore>();

        services.AddSingleton<IPersistenceMigrationRunner, PersistenceMigrationRunner>();

        services.AddSingleton<IPersistenceMigration, VulnerabilityReportsHistoryV2Migration>();
    }
    
    public static async Task RunHistoryMigrationsAsync(this WebApplication app)
    {
        IPersistenceMigrationRunner runner = app.Services.GetRequiredService<IPersistenceMigrationRunner>();

        await runner.RunMigrationsAsync();
    }
}
