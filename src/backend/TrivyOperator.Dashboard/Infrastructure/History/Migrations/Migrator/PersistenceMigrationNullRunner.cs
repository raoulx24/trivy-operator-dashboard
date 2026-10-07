using TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator;

public sealed class PersistenceMigrationNullRunner
    : IPersistenceMigrationRunner
{
    public Task RunMigrationsAsync(CancellationToken ctx = default)
        => Task.CompletedTask;
}
