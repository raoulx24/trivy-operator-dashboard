namespace TrivyOperator.Dashboard.Infrastructure.History.Migrations.Migrator.Models;

public sealed record PersistenceMigrationHistoryDto(
    Status Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string? Error
);
