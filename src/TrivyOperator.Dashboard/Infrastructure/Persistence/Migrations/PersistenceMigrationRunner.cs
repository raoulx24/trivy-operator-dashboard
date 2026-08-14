using StackExchange.Redis;
using System.Text.Json;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.OldEntities;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Entities;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations;

public sealed class PersistenceMigrationRunner(
    IDistributedCacheExecutor executor,
    ICacheEntityCodec cacheEntityCodec,
    IVulnerabilityPersistenceV1Migrator vulnerabilityMigrator,
    ILogger<PersistenceMigrationRunner> logger)
{
    private const string MigrationsKey = "migrations";

    private const string MigrationName = "trivy-vulnerability-persistence-v2";

    private const string MigrationStarted = "started";
    private const string MigrationCompleted = "completed";

    private const string VersionFieldName = "version";
    private const string SnapshotFieldName = "snapshot";

    private const string V1 = "1";
    private const string V2 = "2";

    public async Task RunAsync(CancellationToken ct = default)
    {
        if (await IsMigrationCompleted(ct))
        {
            logger.LogDebug(
                "Persistence migration {migrationName} has already completed.",
                MigrationName);

            return;
        }

        await MarkMigrationStarted(ct);

        try
        {
            await MigrateSnapshots(ct);
            await DeleteUnprocessedSnapshots(ct);

            await MarkMigrationCompleted(ct);

            logger.LogInformation(
                "Persistence migration {migrationName} completed.",
                MigrationName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "Persistence migration {migrationName} failed.",
                MigrationName);

            throw;
        }
    }

    private async Task<bool> IsMigrationCompleted(CancellationToken ct)
    {
        return await executor.ExecuteAsync(
            async db =>
            {
                RedisValue value = await db.HashGetAsync(
                    MigrationsKey,
                    MigrationName);

                return value == MigrationCompleted;
            },
            ct);
    }

    private Task MarkMigrationStarted(CancellationToken ct) =>
        executor.ExecuteAsync(
            db => db.HashSetAsync(
                MigrationsKey,
                MigrationName,
                MigrationStarted),
            ct);

    private Task MarkMigrationCompleted(CancellationToken ct) =>
        executor.ExecuteAsync(
            db => db.HashSetAsync(
                MigrationsKey,
                MigrationName,
                MigrationCompleted),
            ct);

    private Task MigrateSnapshots(CancellationToken ct)
    {
        return executor.ExecuteAsync(
            async db =>
            {
                await foreach (RedisKey key in DistributedCachePrimitives.ScanKeysAsync(
                                   db,
                                   $"{DistributedCacheKeyExtensions.SnapshotKeyPrefix}:*",
                                   ct: ct))
                {
                    ct.ThrowIfCancellationRequested();

                    await MigrateSnapshot(db, key, ct);
                }
            },
            ct);
    }

    private async Task MigrateSnapshot(
        IDatabase db,
        RedisKey key,
        CancellationToken ct)
    {
        RedisValue version = await db.HashGetAsync(
            key,
            VersionFieldName);

        // New/current snapshot.
        if (version == V2)
        {
            return;
        }

        // Missing version means the original persistence format.
        if (!version.IsNullOrEmpty && version != V1)
        {
            throw new InvalidOperationException(
                $"Snapshot '{key}' has unsupported persistence version '{version}'.");
        }

        RedisValue snapshotValue = await db.HashGetAsync(
            key,
            SnapshotFieldName);

        if (snapshotValue.IsNull)
        {
            throw new InvalidOperationException(
                $"Snapshot '{key}' does not contain '{SnapshotFieldName}'.");
        }

        VulnerabilityPersistenceV1[] vulnerabilities =
            DeserializeV1OrEmpty(snapshotValue, key);

        VulnerabilityPersistenceModel[] upgraded =
            vulnerabilities
                .Select(vulnerabilityMigrator.Migrate)
                .ToArray();

        byte[] encoded = cacheEntityCodec.Encode(upgraded);

        // Version is written together with the upgraded snapshot.
        // Therefore a successfully written version 2 always has version-2 data.
        await db.HashSetAsync(
            key,
            [
                new HashEntry(SnapshotFieldName, encoded),
                new HashEntry(VersionFieldName, V2),
            ]);

        logger.LogDebug(
            "Migrated snapshot {snapshotKey} from persistence version {oldVersion} to {newVersion}.",
            key,
            string.IsNullOrEmpty(version) ? V1 : version.ToString(),
            V2);
    }

    private VulnerabilityPersistenceV1[] DeserializeV1OrEmpty(
        RedisValue value,
        RedisKey key)
    {
        try
        {
            byte[] compressed = (byte[])value!;

            using MemoryStream decompressed =
                DistributedCachePrimitives.DecompressToStream(compressed);

            return JsonSerializer.Deserialize<VulnerabilityPersistenceV1[]>(
                       decompressed,
                       new JsonSerializerOptions(JsonSerializerDefaults.Web))
                   ?? [];
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Failed to deserialize V1 vulnerability data for snapshot {snapshotKey}. The vulnerability data will be discarded.",
                key);

            return [];
        }
        catch (InvalidDataException ex)
        {
            logger.LogError(
                ex,
                "Failed to decompress V1 vulnerability data for snapshot {snapshotKey}. " +
                "The vulnerability data will be discarded.",
                key);

            return [];
        }
    }

    private Task DeleteUnprocessedSnapshots(CancellationToken ct)
    {
        return executor.ExecuteAsync(
            async db =>
            {
                await foreach (RedisKey key in DistributedCachePrimitives.ScanKeysAsync(
                                   db,
                                   $"{DistributedCacheKeyExtensions.UnprocessedSnapshotKeyPrefix}:*",
                                   ct: ct))
                {
                    ct.ThrowIfCancellationRequested();

                    await db.KeyDeleteAsync(key);

                    logger.LogDebug(
                        "Deleted unprocessed snapshot {snapshotKey}.",
                        key);
                }
            },
            ct);
    }
}
