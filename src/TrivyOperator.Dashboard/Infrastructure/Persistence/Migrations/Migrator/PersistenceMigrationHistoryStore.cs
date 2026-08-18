using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.Migrations.Migrator;

public sealed class PersistenceMigrationHistoryStore(
    IDistributedCacheExecutor executor) : IPersistenceMigrationHistoryStore
{
    private const string Key = "persistence:migrations";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(), },
    };

    public async Task<PersistenceMigrationHistoryDto?> GetAsync(
        string name,
        CancellationToken ct = default)
    {
        RedisValue value = await executor.ExecuteAsync(
            db => db.HashGetAsync(Key, name),
            ct);

        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<PersistenceMigrationHistoryDto>(
            value.ToString(),
            JsonOptions);
    }

    public async Task SetAsync(
        string name,
        PersistenceMigrationHistoryDto historyDto,
        CancellationToken ct = default)
    {
        string value = JsonSerializer.Serialize(historyDto, JsonOptions);

        await executor.ExecuteAsync(
            db => db.HashSetAsync(Key, name, value),
            ct);
    }
}
