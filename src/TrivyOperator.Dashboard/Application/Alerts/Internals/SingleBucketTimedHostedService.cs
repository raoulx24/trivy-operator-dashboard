using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.Alerts.Services;

namespace TrivyOperator.Dashboard.Application.Alerts.Internals;

public class SingleBucketTimedHostedService(ILogger<SingleBucketTimedHostedService> logger, IAlertPublisher alertPublisher)
    : IHostedService, IDisposable
{
    private const string AlertEmitter = "SingleBucket";
    private readonly HashSet<string> _activeAlerts = [];
    private readonly Random _random = new();
    private Timer? _timer;

    public void Dispose() => _timer?.Dispose();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("TimedAlertService starting.");
        _timer = new Timer(_ => DoWork(cancellationToken), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("TimedAlertService stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void DoWork(CancellationToken cancellationToken)
    {
        bool add = _random.Next(2) == 0;
        string key = $"key-{_random.Next(100)}";

        if (add)
        {
            if (_activeAlerts.Add(key))
            {
                Severity severity = GetRandomSeverity();

                await alertPublisher.AddAlert(
                    AlertEmitter,
                    new Alert
                    {
                        Key = new EmitterKey([key,]),
                        Message = $"SingleBucket and key {key} has something.",
                        Severity = severity,
                        Category = "Test",
                    },
                    cancellationToken
                );
            }
        }
        else
        {
            if (_activeAlerts.Count > 0)
            {
                int index = _random.Next(_activeAlerts.Count);
                string keyToRemove = GetRandomElement(_activeAlerts, index);
                _activeAlerts.Remove(keyToRemove);

                logger.LogInformation($"[-] Alert removed: {keyToRemove}");

                await alertPublisher.RemoveAlert(
                    AlertEmitter,
                    new Alert
                    {
                        Key = new EmitterKey([keyToRemove,]),
                        // message and severity not required for removal
                    },
                    cancellationToken
                );
            }
        }
    }

    private static Severity GetRandomSeverity() => (Severity)new Random().Next(3); // 0 = Info, 1 = Warning, 2 = Error

    private static string GetRandomElement(HashSet<string> set, int index)
    {
        int i = 0;
        foreach (string item in set)
        {
            if (i == index)
            {
                return item;
            }

            i++;
        }

        throw new InvalidOperationException("Index out of range.");
    }
}
