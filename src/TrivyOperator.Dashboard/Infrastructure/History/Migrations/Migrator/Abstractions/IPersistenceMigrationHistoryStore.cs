using TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator.Models;

namespace TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator.Abstractions;

public interface IPersistenceMigrationHistoryStore
{
    Task<PersistenceMigrationHistoryDto?> GetAsync(
        string name,
        CancellationToken ct = default);

    Task SetAsync(
        string name,
        PersistenceMigrationHistoryDto historyDto,
        CancellationToken ct = default);
}
