using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client;

public sealed class DistributedCacheClientFactory : IDistributedCacheClientFactory, IDisposable
{
    private readonly string connectionString;
    private readonly ILogger<DistributedCacheClientFactory> logger;

    private readonly SemaphoreSlim connectionLock = new(1, 1);
    private readonly CancellationTokenSource cts = new();

    private ConnectionMultiplexer? connection;
    private bool disposed;

    public DistributedCacheClientFactory(
        IOptions<DistributedCacheClientOptions> options,
        IHostApplicationLifetime lifetime,
        ILogger<DistributedCacheClientFactory> logger)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ConnectionString))
        {
            throw new ArgumentException("Redis connection string must not be empty.", nameof(connectionString));
        }

        connectionString = options.Value.ConnectionString;
        this.logger = logger;
        
        // Cancel internal CTS when the app is shutting down
        lifetime.ApplicationStopping.Register(() => cts.Cancel());

        _ = Task.Run(EnsureConnectedLoop);
    }

    public IDatabase GetDatabase()
    {
        ThrowIfDisposed();

        ConnectionMultiplexer? conn = Volatile.Read(ref connection);

        return conn is null 
            ? throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis not ready")
            : conn.GetDatabase();
    }

    public ISubscriber GetSubscriber()
    {
        ThrowIfDisposed();

        ConnectionMultiplexer? conn = Volatile.Read(ref connection);

        return conn is null 
            ? throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis not ready") 
            : conn.GetSubscriber();
    }

    private async Task EnsureConnectedLoop()
    {
        TimeSpan delay = TimeSpan.FromSeconds(1);
        TimeSpan baseDelay = TimeSpan.FromSeconds(1);
        TimeSpan maxDelay = TimeSpan.FromSeconds(10);

        while (!cts.Token.IsCancellationRequested)
        {
            ThrowIfDisposed();

            if (Volatile.Read(ref connection) != null)
                return;

            await connectionLock.WaitAsync(cts.Token);
            try
            {
                ThrowIfDisposed();

                if (Volatile.Read(ref connection) != null)
                    return;

                try
                {
                    logger.LogInformation("Connecting to Redis...");

                    await using ConnectionMultiplexer conn = await ConnectionMultiplexer.ConnectAsync(connectionString);

                    WireEvents(conn);

                    Volatile.Write(ref connection, conn);

                    logger.LogInformation("Redis connection established");

                    return;
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Redis unavailable. Retrying in {Delay}...", delay);
                }
            }
            finally
            {
                connectionLock.Release();
            }
            
            delay = Backoff.DecorrelatedJitter(delay, baseDelay, maxDelay);

            try
            {
                await Task.Delay(delay, cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private void WireEvents(ConnectionMultiplexer conn)
    {
        conn.ConnectionFailed += (_, args) =>
        {
            logger.LogInformation("Redis connection failed ({FailureType}) to {EndPoint}", args.FailureType, args.EndPoint);

            if (disposed)
                return;

            // Keep reconnect logic centralized in the background loop.
            // If the connection becomes invalid, clear the current reference
            // so the loop can establish a new one.
            if (args.FailureType is ConnectionFailureType.UnableToConnect or ConnectionFailureType.SocketFailure)
            {
                ConnectionMultiplexer? old = Interlocked.CompareExchange(ref connection, null, conn);

                if (old != conn)
                {
                    return;
                }

                try
                {
                    conn.Dispose();
                }
                catch
                {
                    // swallow during recovery
                }

                Volatile.Write(ref connection, null);
                _ = Task.Run(EnsureConnectedLoop);
            }
        };

        conn.ConnectionRestored += (_, args) =>
        {
            logger.LogInformation("Redis connection restored to {EndPoint}", args.EndPoint);
        };

        conn.ErrorMessage += (_, args) =>
        {
            logger.LogWarning("Redis error: {Message}", args.Message);
        };

        conn.InternalError += (_, args) =>
        {
            logger.LogError("Redis internal error: {Exception}", args.Exception);
        };
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(DistributedCacheClientFactory));
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        try
        {
            cts.Cancel();
        }
        catch
        {
            // ignore shutdown cancellation issues
        }

        try
        {
            ConnectionMultiplexer? conn = Interlocked.Exchange(ref connection, null);
            conn?.Dispose();
        }
        catch
        {
            // ignore shutdown issues
        }

        try
        {
            connectionLock.Dispose();
        }
        catch
        {
            // ignore shutdown issues
        }

        try
        {
            cts.Dispose();
        }
        catch
        {
            // ignore shutdown issues
        }
    }
}