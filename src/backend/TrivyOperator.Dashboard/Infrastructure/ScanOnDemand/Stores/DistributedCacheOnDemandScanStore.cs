using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Factories.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Mappers;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Serializations;
using TrivyOperator.Dashboard.Infrastructure.Trivy.PersistenceModels;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Stores;

// scan records expire ResultTtlHours after creation; saving a result moves the expiry of its scan record to the
// result's, so a result never points to an expired scan
public sealed class DistributedCacheOnDemandScanStore(
    IDistributedCacheExecutor executor,
    ICacheEntityCodec defaultCodec,
    ICacheEntityCodecFactory codecFactory,
    IOptions<ScanOnDemandOptions> options,
    ILogger<DistributedCacheOnDemandScanStore> logger
) : IOnDemandScanStore
{
    private const string LastScanIdField = "lastScanId";
    private const string CollectedAtField = "collectedAt";
    private const string ExpiresAtField = "expiresAt";
    private const string CodecField = "codec";
    private const string VulnerabilityHeaderField = "vr:header";
    private const string VulnerabilityBodyField = "vr:body";
    private const string ExposedSecretHeaderField = "es:header";
    private const string ExposedSecretBodyField = "es:body";
    private const string SbomHeaderField = "sbom:header";
    private const string SbomBodyField = "sbom:body";

    // a record that would expire right away still gets a little time (clock skew, slow scans)
    private static readonly TimeSpan MinimumTtl = TimeSpan.FromMinutes(5);

    public Task SaveScanAsync(OnDemandScan scan, CancellationToken ctx = default)
        => executor.ExecuteAsync(async db =>
        {
            RedisKey key = ScanOnDemandKeys.Scan(scan.Id);
            DateTime expiresAt = scan.CreatedAt.Value.AddHours(options.Value.ResultTtlHours);

            // keep a later expiry set by SaveResultAsync
            TimeSpan? currentTtl = await db.KeyTimeToLiveAsync(key);
            TimeSpan ttl = Max(expiresAt - DateTime.UtcNow, currentTtl ?? TimeSpan.Zero, MinimumTtl);

            await db.StringSetAsync(key, Serialize(scan.ToPersistenceModel(), Json.OnDemandScanPersistenceModel), ttl);
            await db.SetAddAsync(ScanOnDemandKeys.Scans(), scan.Id.Value);
        }, ctx);

    public Task<OnDemandScan?> GetScanAsync(ScanId id, CancellationToken ctx = default)
        => executor.ExecuteAsync(async db =>
        {
            RedisKey key = ScanOnDemandKeys.Scan(id);

            return ReadScan(key, await db.StringGetAsync(key));
        }, ctx);

    public Task<IReadOnlyList<OnDemandScan>> GetScansAsync(CancellationToken ctx = default)
        => executor.ExecuteAsync(async db =>
        {
            RedisKey scansKey = ScanOnDemandKeys.Scans();
            ScanId[] ids = [.. (await db.SetMembersAsync(scansKey)).Select(static x => new ScanId(x.ToString())),];

            // one GET per key, pipelined (no MGET: the keys may live on different cluster slots)
            RedisValue[] values = await Task.WhenAll(ids.Select(x => db.StringGetAsync(ScanOnDemandKeys.Scan(x))));

            List<OnDemandScan> scans = new(ids.Length);
            List<RedisValue> expired = [];

            for (int i = 0; i < ids.Length; i++)
            {
                if (values[i].IsNull)
                {
                    expired.Add(ids[i].Value);
                    continue;
                }

                OnDemandScan? scan = ReadScan(ScanOnDemandKeys.Scan(ids[i]), values[i]);

                if (scan is not null)
                {
                    scans.Add(scan);
                }
            }

            if (expired.Count > 0)
            {
                await db.SetRemoveAsync(scansKey, [.. expired,]);
            }

            return (IReadOnlyList<OnDemandScan>)scans;
        }, ctx);

    public Task DeleteScanAsync(OnDemandScan scan, CancellationToken ctx = default)
        => executor.ExecuteAsync(async db =>
        {
            await db.KeyDeleteAsync(ScanOnDemandKeys.Scan(scan.Id));
            await db.SetRemoveAsync(ScanOnDemandKeys.Scans(), scan.Id.Value);

            if (scan.ImageDigest.IsValid)
            {
                await db.SetRemoveAsync(ScanOnDemandKeys.ResultScans(scan.ImageDigest), scan.Id.Value);
            }
        }, ctx);

    public Task SaveResultAsync(OnDemandScanResult result, CancellationToken ctx = default)
        => executor.ExecuteAsync(async db =>
        {
            RedisKey key = ScanOnDemandKeys.Result(result.ImageDigest);
            RedisKey scansKey = ScanOnDemandKeys.ResultScans(result.ImageDigest);
            DateTime expiresAt = Max(result.ExpiresAt.Value, DateTime.UtcNow.Add(MinimumTtl));

            List<HashEntry> entries =
            [
                new(LastScanIdField, result.LastScanId.Value),
                new(CollectedAtField, result.CollectedAt.ToString()),
                new(ExpiresAtField, result.ExpiresAt.ToString()),
                new(CodecField, defaultCodec.CodecName),
            ];

            if (result.VulnerabilityReport is not null)
            {
                (VulnerabilityReportHeaderPersistenceModel header, VulnerabilityPersistenceModel[] body) =
                    result.VulnerabilityReport.ToPersistenceModels();
                entries.Add(new HashEntry(VulnerabilityHeaderField, Serialize(header, Json.VulnerabilityReportHeaderPersistenceModel)));
                entries.Add(new HashEntry(VulnerabilityBodyField, defaultCodec.Encode(body)));
            }

            if (result.ExposedSecretReport is not null)
            {
                (ExposedSecretReportHeaderPersistenceModel header, SecretPersistenceModel[] body) =
                    result.ExposedSecretReport.ToPersistenceModels();
                entries.Add(new HashEntry(ExposedSecretHeaderField, Serialize(header, Json.ExposedSecretReportHeaderPersistenceModel)));
                entries.Add(new HashEntry(ExposedSecretBodyField, defaultCodec.Encode(body)));
            }

            if (result.SbomReport is not null)
            {
                (SbomReportHeaderPersistenceModel header, ComponentPersistenceModel[] body) =
                    result.SbomReport.ToPersistenceModels();
                entries.Add(new HashEntry(SbomHeaderField, Serialize(header, Json.SbomReportHeaderPersistenceModel)));
                entries.Add(new HashEntry(SbomBodyField, defaultCodec.Encode(body)));
            }

            // the result replaces the previous one of the digest as a whole
            ITransaction transaction = db.CreateTransaction();
            _ = transaction.KeyDeleteAsync(key);
            _ = transaction.HashSetAsync(key, [.. entries,]);
            _ = transaction.KeyExpireAsync(key, expiresAt);
            _ = transaction.SetAddAsync(scansKey, result.LastScanId.Value);
            _ = transaction.KeyExpireAsync(scansKey, expiresAt);

            if (!await transaction.ExecuteAsync())
            {
                throw new InvalidOperationException($"Saving the scan result of {result.ImageDigest} was not committed.");
            }

            // outside the transaction: another slot; read and compare instead of EXPIRE GT (Redis 7+ only)
            RedisKey scanKey = ScanOnDemandKeys.Scan(result.LastScanId);
            TimeSpan? scanTtl = await db.KeyTimeToLiveAsync(scanKey);

            if (scanTtl is not null && DateTime.UtcNow.Add(scanTtl.Value) < expiresAt)
            {
                await db.KeyExpireAsync(scanKey, expiresAt);
            }
        }, ctx);

    public Task<OnDemandScanResult?> GetResultAsync(Digest digest, CancellationToken ctx = default)
        => executor.ExecuteAsync(async db =>
        {
            RedisKey key = ScanOnDemandKeys.Result(digest);
            Dictionary<string, RedisValue> fields = (await db.HashGetAllAsync(key))
                .ToDictionary(static x => x.Name.ToString(), static x => x.Value, StringComparer.Ordinal);

            if (fields.Count == 0)
            {
                return null;
            }

            try
            {
                return ReadResult(digest, fields);
            }
            catch (Exception ex) when (ex is FormatException or JsonException or InvalidOperationException or ArgumentException)
            {
                logger.LogError(ex, "Scan result in key {distributedCacheKey} could not be read", key.ToString());
                return null;
            }
        }, ctx);

    public Task DeleteResultAsync(Digest digest, CancellationToken ctx = default)
        => executor.ExecuteAsync(async db =>
        {
            RedisKey scansKey = ScanOnDemandKeys.ResultScans(digest);
            RedisValue[] scanIds = await db.SetMembersAsync(scansKey);

            foreach (RedisValue scanId in scanIds)
            {
                await db.KeyDeleteAsync(ScanOnDemandKeys.Scan(new ScanId(scanId.ToString())));
            }

            if (scanIds.Length > 0)
            {
                await db.SetRemoveAsync(ScanOnDemandKeys.Scans(), scanIds);
            }

            await db.KeyDeleteAsync([ScanOnDemandKeys.Result(digest), scansKey,]);
        }, ctx);

    private OnDemandScan? ReadScan(RedisKey key, RedisValue value)
    {
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        try
        {
            OnDemandScanPersistenceModel? dto = JsonSerializer.Deserialize((byte[])value!, Json.OnDemandScanPersistenceModel);

            return dto?.ToDomain();
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            logger.LogError(ex, "Scan record in key {distributedCacheKey} could not be read", key.ToString());
            return null;
        }
    }

    private OnDemandScanResult ReadResult(Digest digest, Dictionary<string, RedisValue> fields)
    {
        string codecName = fields.TryGetValue(CodecField, out RedisValue codecValue) && !codecValue.IsNullOrEmpty
            ? codecValue.ToString()
            : defaultCodec.CodecName;
        ICacheEntityCodec codec = string.Equals(codecName, defaultCodec.CodecName, StringComparison.Ordinal)
            ? defaultCodec
            : codecFactory.GetCacheEntityCodec(codecName);

        VulnerabilityReport? vulnerabilityReport =
            ReadReport<VulnerabilityReportHeaderPersistenceModel, VulnerabilityPersistenceModel, VulnerabilityReport>(
                fields, VulnerabilityHeaderField, VulnerabilityBodyField, Json.VulnerabilityReportHeaderPersistenceModel,
                static (h, b) => h.ToDomain(b), codec);

        ExposedSecretReport? exposedSecretReport =
            ReadReport<ExposedSecretReportHeaderPersistenceModel, SecretPersistenceModel, ExposedSecretReport>(
                fields, ExposedSecretHeaderField, ExposedSecretBodyField, Json.ExposedSecretReportHeaderPersistenceModel,
                static (h, b) => h.ToDomain(b), codec);

        SbomReport? sbomReport =
            ReadReport<SbomReportHeaderPersistenceModel, ComponentPersistenceModel, SbomReport>(
                fields, SbomHeaderField, SbomBodyField, Json.SbomReportHeaderPersistenceModel,
                static (h, b) => h.ToDomain(b), codec);

        return new OnDemandScanResult(
            digest,
            new ScanId(fields.GetValueOrDefault(LastScanIdField).ToString()),
            ReadTimestamp(fields, CollectedAtField),
            ReadTimestamp(fields, ExpiresAtField),
            vulnerabilityReport,
            exposedSecretReport,
            sbomReport);
    }

    private static TReport? ReadReport<THeader, TBody, TReport>(
        Dictionary<string, RedisValue> fields,
        string headerField,
        string bodyField,
        JsonTypeInfo<THeader> headerTypeInfo,
        Func<THeader, TBody[], TReport> toDomain,
        ICacheEntityCodec codec)
        where TReport : class
    {
        if (!fields.TryGetValue(headerField, out RedisValue headerValue) || headerValue.IsNullOrEmpty
            || !fields.TryGetValue(bodyField, out RedisValue bodyValue) || bodyValue.IsNullOrEmpty)
        {
            return null;
        }

        THeader header = JsonSerializer.Deserialize((byte[])headerValue!, headerTypeInfo)
                         ?? throw new FormatException($"Field {headerField} is empty.");
        TBody[] body = codec.Decode<TBody[]>((byte[])bodyValue!);

        return toDomain(header, body);
    }

    private static Timestamp ReadTimestamp(Dictionary<string, RedisValue> fields, string field)
        => fields.TryGetValue(field, out RedisValue value)
           && DateTime.TryParse(value.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dateTime)
            ? new Timestamp(dateTime)
            : throw new FormatException($"Field {field} is missing or not a timestamp.");

    private static byte[] Serialize<T>(T value, JsonTypeInfo<T> typeInfo)
        => DistributedCachePrimitives.SerializeJson(value, typeInfo);

    private static TimeSpan Max(params TimeSpan[] values) => values.Max();

    private static DateTime Max(DateTime left, DateTime right) => left > right ? left : right;

    private static ScanOnDemandPersistenceJsonContext Json => ScanOnDemandPersistenceJsonContext.Default;
}
