namespace TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator.Abstractions;

public interface IPersistenceMigrationRunner
{
    Task RunMigrationsAsync(CancellationToken ct = default);
}

