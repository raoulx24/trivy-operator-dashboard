using StackExchange.Redis;
using System.IO.Compression;
using System.Text.Json;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client;

public static class DistributedCachePrimitives
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // ------------------------------------------------------------------------
    // Low-level Redis access
    // ------------------------------------------------------------------------

    private static async Task<RedisValue> GetFieldAsync(
        IDatabase db,
        RedisKey key,
        RedisValue field,
        CancellationToken ct = default)
    {
        // StackExchange.Redis does not support CancellationToken directly,
        // but we keep it in the signature for future-proofing / symmetry.
        return await db.HashGetAsync(key, field);
    }

    public static async Task<byte[]?> GetBytesFieldAsync(
        IDatabase db,
        RedisKey key,
        RedisValue field,
        ILogger logger,
        CancellationToken ct = default)
    {
        RedisValue value = await GetFieldAsync(db, key, field, ct);

        if (value.IsNullOrEmpty)
            return null;

        try
        {
            return (byte[])value!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to cast RedisValue to byte[] for key {distributedCacheKey}, field {field}",
                key.ToString(), field.ToString());
            return null;
        }
    }

    // ------------------------------------------------------------------------
    // Mid-level JSON helpers
    // ------------------------------------------------------------------------

    public static async Task<T?> GetJsonAsync<T>(
        IDatabase db,
        RedisKey key,
        RedisValue field,
        ILogger logger,
        CancellationToken ct = default)
    {
        RedisValue value = await GetFieldAsync(db, key, field, ct);

        if (value.IsNull)
            return default;

        if (value.IsNullOrEmpty)
        {
            logger.LogError(
                "Field {field} in key {distributedCacheKey} is null or empty when expecting JSON for type {type}",
                field.ToString(), key.ToString(), typeof(T).FullName);
            return default;
        }

        try
        {
            string json = value.ToString();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to deserialize JSON for key {distributedCacheKey}, field {field}, type {type}",
                key.ToString(), field.ToString(), typeof(T).FullName);
            return default;
        }
    }

    public static async Task<T?> GetCompressedJsonAsync<T>(
        IDatabase db,
        RedisKey key,
        RedisValue field,
        ILogger logger,
        CancellationToken ct = default)
    {
        byte[]? bytes = await GetBytesFieldAsync(db, key, field, logger, ct);
        if (bytes is null)
            return default;

        try
        {
            using MemoryStream decompressed = DecompressToStream(bytes);
            return await JsonSerializer.DeserializeAsync<T>(decompressed, JsonOptions, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to decompress or deserialize compressed JSON for key {distributedCacheKey}, field {field}, type {type}",
                key.ToString(), field.ToString(), typeof(T).FullName);
            return default;
        }
    }

    // ------------------------------------------------------------------------
    // High-level typed helpers
    // ------------------------------------------------------------------------

    public static async Task<string?> GetHSetStringAsync(
        IDatabase db,
        RedisKey key,
        RedisValue field,
        ILogger logger,
        CancellationToken ct = default)
    {
        RedisValue value = await GetFieldAsync(db, key, field, ct);

        if (value.IsNull)
            return null;

        if (value.IsNullOrEmpty)
        {
            logger.LogError(
                "Field {field} in key {distributedCacheKey} is null or empty when expecting string",
                field.ToString(), key.ToString());
            return null;
        }

        try
        {
            return value.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to convert RedisValue to string for key {distributedCacheKey}, field {field}",
                key.ToString(), field.ToString());
            return null;
        }
    }

    public static async Task<DateTime?> GetHSetTimestampAsync(
        IDatabase db,
        RedisKey key,
        RedisValue field,
        ILogger logger,
        CancellationToken ct = default)
    {
        string? value = await GetHSetStringAsync(db, key, field, logger, ct);
        if (value is null)
            return null;

        if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime parsed))
        {
            return parsed;
        }

        logger.LogError(
            "Invalid timestamp format in key {distributedCacheKey}, field {field}, value {value}",
            key.ToString(), field.ToString(), value);
        return null;
    }

    public static async Task<bool?> GetHSetBoolAsync(
        IDatabase db,
        RedisKey key,
        RedisValue field,
        ILogger logger,
        CancellationToken ct = default)
    {
        string? value = await GetHSetStringAsync(db, key, field, logger, ct);
        if (value is null)
            return null;

        value = value.Trim();

        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            return false;
        if (value == "1")
            return true;
        if (value == "0")
            return false;

        logger.LogError(
            "Invalid boolean format in key {distributedCacheKey}, field {field}, value {value}",
            key.ToString(), field.ToString(), value);
        return null;
    }
    
    public static async Task<IReadOnlyList<string>> GetSetMembersAsync(
        IDatabase db,
        RedisKey key,
        ILogger logger,
        CancellationToken ct = default)
    {
        RedisValue[] values = await db.SetMembersAsync(key);

        List<string> result = [];

        foreach (RedisValue value in values)
        {
            if (value.IsNullOrEmpty)
            {
                logger.LogError(
                    "Set member in key {distributedCacheKey} is null or empty",
                    key.ToString());

                continue;
            }

            result.Add(value.ToString());
        }

        return result;
    }
    
    public static async Task AddToSetAsync(
        IDatabase db,
        RedisKey key,
        string value,
        CancellationToken ct = default)
    {
        await db.SetAddAsync(key, value);
    }
    
    public static async Task RemoveFromSetAsync(
        IDatabase db,
        RedisKey key,
        IEnumerable<string> values,
        CancellationToken ct = default)
    {
        RedisValue[] redisValues = values
            .Select(x => (RedisValue)x)
            .ToArray();

        if (redisValues.Length == 0)
            return;

        await db.SetRemoveAsync(key, redisValues);
    }

    // ------------------------------------------------------------------------
    // Serialization helpers (for writing)
    // ------------------------------------------------------------------------

    public static byte[] SerializeJson<T>(T value)
        => JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

    public static byte[] SerializeJsonCompressed<T>(T value)
    {
        byte[] jsonBytes = SerializeJson(value);
        return CompressToBrotli(jsonBytes);
    }

    // ------------------------------------------------------------------------
    // SCAN helper
    // ------------------------------------------------------------------------

    public static async IAsyncEnumerable<RedisKey> ScanKeysAsync(
        IDatabase db,
        string pattern,
        int pageSize = 1000,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be positive.");

        string cursor = "0";

        do
        {
            ct.ThrowIfCancellationRequested();

            RedisResult result = await db.ExecuteAsync(
                "SCAN",
                cursor,
                "MATCH", pattern,
                "COUNT", pageSize).ConfigureAwait(false);

            // SCAN returns: [ newCursor, [keys...] ]
            RedisResult[] outer = (RedisResult[])result!;

            cursor = (string)outer[0]!;

            RedisResult[] items = (RedisResult[])outer[1]!;

            foreach (RedisResult item in items)
            {
                ct.ThrowIfCancellationRequested();

                if (item.Resp2Type == ResultType.BulkString)
                    yield return (string)item!;
            }

        } while (cursor != "0");
    }

    public static async Task<bool> KeyPatternExistsAsync(
        IDatabase db,
        string pattern,
        int pageSize = 1000,
        CancellationToken ct = default)
    {
        await foreach (var key in ScanKeysAsync(db, pattern, pageSize, ct)
                           .WithCancellation(ct)
                           .ConfigureAwait(false))
        {
            // If we get even one key, the pattern exists
            return true;
        }

        return false;
    }
    
    // ------------------------------------------------------------------------
    // DELETE keys by pattern helper
    // ------------------------------------------------------------------------
    
    public static async Task DeleteKeysAsync(
        IDatabase db,
        string pattern,
        int pageSize = 1000,
        CancellationToken ct = default)
    {
        List<RedisKey> keys = [];

        await foreach (RedisKey key in ScanKeysAsync(
                           db,
                           pattern,
                           pageSize,
                           ct))
        {
            keys.Add(key);

            if (keys.Count >= pageSize)
            {
                await db.KeyDeleteAsync(keys.ToArray()).ConfigureAwait(false);
                keys.Clear();
            }
        }

        if (keys.Count > 0)
        {
            await db.KeyDeleteAsync(keys.ToArray()).ConfigureAwait(false);
        }
    }


    // ------------------------------------------------------------------------
    // Brotli helpers
    // ------------------------------------------------------------------------

    private static byte[] CompressToBrotli(byte[] data)
    {
        using MemoryStream output = new();
        using (BrotliStream brotli = new(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            brotli.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    public static MemoryStream DecompressToStream(byte[] data)
    {
        MemoryStream output = new();

        using (MemoryStream input = new(data))
        using (BrotliStream brotli = new(input, CompressionMode.Decompress))
        {
            brotli.CopyTo(output);
        }

        output.Position = 0;
        return output;
    }
}
