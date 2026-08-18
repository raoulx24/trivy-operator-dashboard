namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator.Models;

public sealed record PersistenceMigrationHistoryDto(
    Status Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string? Error
);
