namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator.Abstractions;

public interface IPersistenceMigrationRunner
{
    Task RunMigrationsAsync(CancellationToken ct = default);
}

