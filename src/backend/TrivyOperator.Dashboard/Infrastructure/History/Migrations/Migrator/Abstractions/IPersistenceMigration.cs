namespace TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator.Abstractions;

public interface IPersistenceMigration
{
    Task RunAsync(CancellationToken ctx = default);

    uint Order { get; }
    string Name { get; }
}
