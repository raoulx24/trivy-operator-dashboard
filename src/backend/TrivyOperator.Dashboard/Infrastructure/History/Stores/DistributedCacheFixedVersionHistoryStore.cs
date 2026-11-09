using StackExchange.Redis;
using System.Globalization;
using System.Text.Json;
using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;
using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.History.Models;
using TrivyOperator.Dashboard.Infrastructure.History.Serializations;

namespace TrivyOperator.Dashboard.Infrastructure.History.Stores;

// Redis layout
// - fv:{vulnerabilityId|packageKey}  hash
//     fixedVersion  latest value ("" = no fix)
//     at            latest entry's report updateTimestamp (O format)
//     touchedAt     last time an event saw the pair (O format)
//     entries       JSON array of {fixedVersion, at}, oldest first; the first one is the baseline
// - fv:changes  sorted set of the changes (not the baselines): member "vulnerabilityId|packageKey|at", score = at (unix ms)
// the vulnerability id never contains '|', the package key may (fallback "name|target"): split at the first '|'
public sealed class DistributedCacheFixedVersionHistoryStore(
    IDistributedCacheExecutor executor,
    ILogger<DistributedCacheFixedVersionHistoryStore> logger
) : IFixedVersionHistoryStore
{
    private const string KeyPrefix = "fv:{";
    private const string KeySuffix = "}";
    private const string AllKeysPattern = "fv:{*}";
    private static readonly RedisKey ChangesKey = "fv:changes";

    private const string FixedVersionFieldName = "fixedVersion";
    private const string AtFieldName = "at";
    private const string TouchedAtFieldName = "touchedAt";
    private const string EntriesFieldName = "entries";

    private static readonly RedisValue[] StateFields = [FixedVersionFieldName, AtFieldName, TouchedAtFieldName,];

    public async Task<IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionState>> GetStatesAsync(
        IReadOnlyCollection<PackageVulnerabilityKey> keys,
        CancellationToken ctx = default)
    {
        if (keys.Count == 0)
        {
            return new Dictionary<PackageVulnerabilityKey, FixedVersionState>();
        }

        return await executor.ExecuteAsync(async db =>
        {
            IBatch batch = db.CreateBatch();
            List<(PackageVulnerabilityKey Key, Task<RedisValue[]> Values)> reads =
            [
                .. keys.Select(key => (key, batch.HashGetAsync(ToRedisKey(key), StateFields))),
            ];
            batch.Execute();
            await Task.WhenAll(reads.Select(x => x.Values));

            Dictionary<PackageVulnerabilityKey, FixedVersionState> result = [];

            foreach ((PackageVulnerabilityKey key, Task<RedisValue[]> values) in reads)
            {
                FixedVersionState? state = ToState(key, values.Result);

                if (state is not null)
                {
                    result[key] = state;
                }
            }

            return (IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionState>)result;
        }, ctx);
    }

    public async Task ApplyAsync(IReadOnlyCollection<FixedVersionWrite> writes, CancellationToken ctx = default)
    {
        if (writes.Count == 0)
        {
            return;
        }

        await executor.ExecuteAsync(async db =>
        {
            // a change appends to the stored entries, so they are read first (one round trip)
            FixedVersionWrite[] changes = [.. writes.Where(x => x.Kind == FixedVersionWriteKind.Change),];
            Dictionary<PackageVulnerabilityKey, FixedVersionEntryPersistenceModel[]> storedEntries = [];

            if (changes.Length > 0)
            {
                IBatch readBatch = db.CreateBatch();
                List<(PackageVulnerabilityKey Key, Task<RedisValue> Value)> reads =
                [
                    .. changes.Select(x => (x.Key, readBatch.HashGetAsync(ToRedisKey(x.Key), EntriesFieldName))),
                ];
                readBatch.Execute();
                await Task.WhenAll(reads.Select(x => x.Value));

                foreach ((PackageVulnerabilityKey key, Task<RedisValue> value) in reads)
                {
                    storedEntries[key] = DeserializeEntries(value.Result, key);
                }
            }

            IBatch writeBatch = db.CreateBatch();
            List<Task> pending = [];

            foreach (FixedVersionWrite write in writes)
            {
                RedisKey redisKey = ToRedisKey(write.Key);
                string touchedAt = FormatTimestamp(write.TouchedAt);

                switch (write.Kind)
                {
                    case FixedVersionWriteKind.Touch:
                        pending.Add(writeBatch.HashSetAsync(redisKey, TouchedAtFieldName, touchedAt));
                        break;

                    case FixedVersionWriteKind.Baseline:
                    case FixedVersionWriteKind.Change:
                        FixedVersionEntry entry = write.Entry!;
                        FixedVersionEntryPersistenceModel[] entries = write.Kind == FixedVersionWriteKind.Baseline
                            ? [ToPersistenceModel(entry),]
                            : [.. storedEntries.GetValueOrDefault(write.Key, []), ToPersistenceModel(entry),];

                        pending.Add(writeBatch.HashSetAsync(redisKey,
                        [
                            new HashEntry(FixedVersionFieldName, entry.FixedVersion?.Value ?? string.Empty),
                            new HashEntry(AtFieldName, FormatTimestamp(entry.At)),
                            new HashEntry(TouchedAtFieldName, touchedAt),
                            new HashEntry(EntriesFieldName, SerializeEntries(entries)),
                        ]));

                        if (write.Kind == FixedVersionWriteKind.Change)
                        {
                            pending.Add(writeBatch.SortedSetAddAsync(
                                ChangesKey,
                                ToChangeMember(write.Key, entry.At),
                                ToScore(entry.At)));
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(writes), write.Kind, "Unknown fixed version write kind.");
                }
            }

            writeBatch.Execute();
            await Task.WhenAll(pending);
        }, ctx);
    }

    public async Task<IReadOnlyList<FixedVersionEntry>> GetEntriesAsync(
        PackageVulnerabilityKey key,
        CancellationToken ctx = default)
    {
        return await executor.ExecuteAsync(async db =>
        {
            RedisValue value = await db.HashGetAsync(ToRedisKey(key), EntriesFieldName);

            return (IReadOnlyList<FixedVersionEntry>)[.. DeserializeEntries(value, key).Select(ToDomain),];
        }, ctx);
    }

    public async Task<IReadOnlyList<FixedVersionChange>> GetChangesAsync(Timestamp from, CancellationToken ctx = default)
    {
        return await executor.ExecuteAsync(async db =>
        {
            RedisValue[] members = await db.SortedSetRangeByScoreAsync(ChangesKey, ToScore(from));

            List<FixedVersionChange> result = [];

            foreach (RedisValue member in members)
            {
                FixedVersionChange? change = ParseChangeMember(member.ToString());

                if (change is null)
                {
                    logger.LogWarning("Invalid member {member} in {distributedCacheKey}. Skipping", member.ToString(), ChangesKey.ToString());
                    continue;
                }

                result.Add(change);
            }

            return (IReadOnlyList<FixedVersionChange>)result;
        }, ctx);
    }

    public async Task<IReadOnlyList<FixedVersionState>> GetAllStatesAsync(CancellationToken ctx = default)
    {
        return await executor.ExecuteAsync(async db =>
        {
            List<FixedVersionState> result = [];

            await foreach (RedisKey redisKey in DistributedCachePrimitives.ScanKeysAsync(db, AllKeysPattern, ct: ctx))
            {
                ctx.ThrowIfCancellationRequested();

                PackageVulnerabilityKey? key = ParseRedisKey(redisKey.ToString());

                if (key is null)
                {
                    logger.LogWarning("Key {distributedCacheKey} is not a fixed version history key. Skipping", redisKey.ToString());
                    continue;
                }

                RedisValue[] values = await db.HashGetAsync(redisKey, StateFields);
                FixedVersionState? state = ToState(key.Value, values);

                if (state is not null)
                {
                    result.Add(state);
                }
            }

            return (IReadOnlyList<FixedVersionState>)result;
        }, ctx);
    }

    public async Task<bool> DeleteIfUntouchedAsync(FixedVersionState state, CancellationToken ctx = default)
    {
        return await executor.ExecuteAsync(async db =>
        {
            RedisKey redisKey = ToRedisKey(state.Key);

            RedisValue[] values = await db.HashGetAsync(redisKey, [EntriesFieldName, TouchedAtFieldName,]);
            RedisValue storedTouchedAt = values[1];

            // touched since the caller read the state: an event saw the pair in between, keep it
            if (ParseTimestamp(storedTouchedAt) is { } touchedAt && touchedAt > state.TouchedAt.Value)
            {
                return false;
            }

            // the changes of the pair are removed with it; their members are rebuilt from the entries
            RedisValue[] changeMembers =
            [
                .. DeserializeEntries(values[0], state.Key)
                    .Skip(1)
                    .Select(x => (RedisValue)ToChangeMember(state.Key, new Timestamp(x.At))),
            ];

            // conditional on the touch just read: an event between the read and the delete keeps the pair
            ITransaction transaction = db.CreateTransaction();
            transaction.AddCondition(storedTouchedAt.IsNull
                ? Condition.HashNotExists(redisKey, TouchedAtFieldName)
                : Condition.HashEqual(redisKey, TouchedAtFieldName, storedTouchedAt));
            _ = transaction.KeyDeleteAsync(redisKey);

            if (changeMembers.Length > 0)
            {
                _ = transaction.SortedSetRemoveAsync(ChangesKey, changeMembers);
            }

            return await transaction.ExecuteAsync();
        }, ctx);
    }

    // ------------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------------

    private FixedVersionState? ToState(PackageVulnerabilityKey key, RedisValue[] values)
    {
        // all fields missing: the pair is not stored
        if (values.All(x => x.IsNull))
        {
            return null;
        }

        DateTime? at = ParseTimestamp(values[1]);
        DateTime? touchedAt = ParseTimestamp(values[2]);

        if (at is null)
        {
            logger.LogError("Field {field} of fixed version history {key} is missing or invalid", AtFieldName, key.ToString());
            return null;
        }

        FixedVersionEntry latest = new(
            values[0].IsNullOrEmpty ? null : new PackageVersion(values[0].ToString()),
            new Timestamp(at.Value));

        // a missing touch is treated as the latest entry's moment (the housekeeping grace then starts from there)
        return new FixedVersionState(key, latest, new Timestamp(touchedAt ?? at.Value));
    }

    private FixedVersionEntryPersistenceModel[] DeserializeEntries(RedisValue value, PackageVulnerabilityKey key)
    {
        if (value.IsNullOrEmpty)
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize(
                value.ToString(),
                HistoryPersistenceJsonContext.Default.FixedVersionEntryPersistenceModelArray) ?? [];
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Entries of fixed version history {key} could not be deserialized", key.ToString());
            return [];
        }
    }

    private static string SerializeEntries(FixedVersionEntryPersistenceModel[] entries) =>
        JsonSerializer.Serialize(entries, HistoryPersistenceJsonContext.Default.FixedVersionEntryPersistenceModelArray);

    private static FixedVersionEntryPersistenceModel ToPersistenceModel(FixedVersionEntry entry) =>
        new(entry.FixedVersion?.Value, entry.At.Value.ToUniversalTime());

    private static FixedVersionEntry ToDomain(FixedVersionEntryPersistenceModel model) =>
        new(model.FixedVersion is null ? null : new PackageVersion(model.FixedVersion), new Timestamp(model.At));

    private static RedisKey ToRedisKey(PackageVulnerabilityKey key) =>
        $"{KeyPrefix}{FormatKey(key)}{KeySuffix}";

    private static string FormatKey(PackageVulnerabilityKey key) =>
        $"{key.VulnerabilityId.Value}|{key.PackageKey.Value}";

    private static PackageVulnerabilityKey? ParseRedisKey(string redisKey)
    {
        if (!redisKey.StartsWith(KeyPrefix, StringComparison.Ordinal) ||
            !redisKey.EndsWith(KeySuffix, StringComparison.Ordinal))
        {
            return null;
        }

        return ParseKey(redisKey[KeyPrefix.Length..^KeySuffix.Length]);
    }

    private static PackageVulnerabilityKey? ParseKey(string value)
    {
        int separator = value.IndexOf('|');

        if (separator <= 0 || separator == value.Length - 1)
        {
            return null;
        }

        return new PackageVulnerabilityKey(
            new VulnerabilityId(value[..separator]),
            new PackageKey(value[(separator + 1)..]));
    }

    private static string ToChangeMember(PackageVulnerabilityKey key, Timestamp at) =>
        $"{FormatKey(key)}|{FormatTimestamp(at)}";

    // the moment is after the last '|' (an O timestamp has none)
    private static FixedVersionChange? ParseChangeMember(string member)
    {
        int separator = member.LastIndexOf('|');

        if (separator <= 0)
        {
            return null;
        }

        PackageVulnerabilityKey? key = ParseKey(member[..separator]);
        DateTime? at = ParseTimestamp(member[(separator + 1)..]);

        return key is null || at is null ? null : new FixedVersionChange(key.Value, new Timestamp(at.Value));
    }

    private static string FormatTimestamp(Timestamp timestamp) =>
        timestamp.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTime? ParseTimestamp(RedisValue value) =>
        !value.IsNullOrEmpty &&
        DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed)
            ? parsed
            : null;

    private static double ToScore(Timestamp timestamp) =>
        new DateTimeOffset(timestamp.Value.ToUniversalTime()).ToUnixTimeMilliseconds();
}
