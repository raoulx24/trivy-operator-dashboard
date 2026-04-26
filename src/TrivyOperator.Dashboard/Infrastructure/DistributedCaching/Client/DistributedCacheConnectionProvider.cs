using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCaching.Client;

public sealed class DistributedCacheConnectionProvider(
    IOptions<DistributedCacheClientOptions> options,
    ILogger<DistributedCacheConnectionProvider> logger) : IHostedService, IDisposable
{
    private readonly string connectionString = options.Value.ConnectionString;

    private readonly SemaphoreSlim initLock = new(1, 1);
    private ConnectionMultiplexer? connection;
    private Task? initTask;
    private bool disposed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Kick off initialization once
        initTask = EnsureConnectedAsync(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public async Task<ConnectionMultiplexer> GetAsync(CancellationToken ct = default)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(DistributedCacheConnectionProvider));

        if (connection != null)
            return connection;

        await EnsureConnectedAsync(ct);

        return connection 
            ?? throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Distributed Cache not available");
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (connection != null)
            return;

        await initLock.WaitAsync(ct);
        try
        {
            if (connection != null)
                return;

            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Connecting to Distributed Cache...");

                    ConnectionMultiplexer conn = await ConnectionMultiplexer.ConnectAsync(connectionString);

                    WireEvents(conn);

                    connection = conn;

                    logger.LogInformation("Distributed Cache connected");
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Distributed Cache unavailable, retrying in {Delay}", delay);
                    await Task.Delay(delay, ct);

                    // simple backoff (good enough)
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10));
                }
            }
        }
        finally
        {
            initLock.Release();
        }
    }

    private void WireEvents(ConnectionMultiplexer conn)
    {
        conn.ConnectionFailed += (_, e) =>
            logger.LogWarning("Distributed Cache connection failed: {FailureType} {EndPoint}", e.FailureType, e.EndPoint);

        conn.ConnectionRestored += (_, e) =>
            logger.LogInformation("Distributed Cache connection restored: {EndPoint}", e.EndPoint);

        conn.ErrorMessage += (_, e) =>
            logger.LogWarning("Distributed Cache error: {Message}", e.Message);

        conn.InternalError += (_, e) =>
            logger.LogError(e.Exception, "Distributed Cache internal error");
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        try { connection?.Dispose(); } catch { }
        try { initLock.Dispose(); } catch { }
    }
}
