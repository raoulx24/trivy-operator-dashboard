using TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator;

public sealed class PersistenceMigrationRunner(
    IEnumerable<IPersistenceMigration> migrations,
    IPersistenceMigrationHistoryStore historyStore,
    ILogger<PersistenceMigrationRunner> logger)
    : IPersistenceMigrationRunner
{
    public async Task RunMigrationsAsync(CancellationToken ct = default)
    {
        var orderedMigrations = PrepareMigrations();

        foreach (var migration in orderedMigrations)
        {
            ct.ThrowIfCancellationRequested();

            var previous = await historyStore.GetAsync(migration.Name, ct);

            if (previous?.Status == Status.Finished)
            {
                logger.LogDebug(
                    "Skipping finished persistence migration {MigrationName}.",
                    migration.Name);

                continue;
            }

            await RunMigrationAsync(migration, ct);
        }
    }

    private async Task RunMigrationAsync(
        IPersistenceMigration migration,
        CancellationToken ct)
    {
        var startedAt = DateTimeOffset.UtcNow;

        await historyStore.SetAsync(
            migration.Name,
            new PersistenceMigrationHistoryDto(
                Status.Started,
                startedAt,
                FinishedAt: null,
                Error: null),
            ct);

        logger.LogInformation(
            "Running persistence migration {MigrationName}.",
            migration.Name);

        try
        {
            await migration.RunAsync(ct);

            await historyStore.SetAsync(
                migration.Name,
                new PersistenceMigrationHistoryDto(
                    Status.Finished,
                    startedAt,
                    DateTimeOffset.UtcNow,
                    Error: null),
                ct);

            logger.LogInformation(
                "Finished persistence migration {MigrationName}.",
                migration.Name);
        }
        catch (Exception ex)
        {
            try
            {
                await historyStore.SetAsync(
                    migration.Name,
                    new PersistenceMigrationHistoryDto(
                        Status.Error,
                        startedAt,
                        FinishedAt: null,
                        Error: ex.Message),
                    CancellationToken.None);
            }
            catch (Exception historyException)
            {
                logger.LogError(
                    historyException,
                    "Failed to record error state for persistence migration {MigrationName}.",
                    migration.Name);
            }

            logger.LogError(
                ex,
                "Persistence migration {MigrationName} failed.",
                migration.Name);

            throw;
        }
    }

    private IPersistenceMigration[] PrepareMigrations()
    {
        var materialized = migrations.ToArray();

        var duplicate = materialized
            .GroupBy(x => x.Name, StringComparer.Ordinal)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Duplicate persistence migration name '{duplicate.Key}'.");
        }

        foreach (var migration in materialized)
        {
            if (string.IsNullOrWhiteSpace(migration.Name))
            {
                throw new InvalidOperationException(
                    "A persistence migration has an empty name.");
            }
        }

        return materialized
            .GroupBy(x => x.Order)
            .OrderBy(x => x.Key)
            .SelectMany(x => x.OrderBy(_ => Random.Shared.Next()))
            .ToArray();
    }
}
