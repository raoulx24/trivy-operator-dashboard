namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator.Abstractions;

public interface IPersistenceMigration
{
    Task RunAsync(CancellationToken ctx = default);

    uint Order { get; }
    string Name { get; }
}
