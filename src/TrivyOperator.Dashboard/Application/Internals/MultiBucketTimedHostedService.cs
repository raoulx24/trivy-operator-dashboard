﻿using TrivyOperator.Dashboard.Application.Services.Alerts;
using TrivyOperator.Dashboard.Application.Services.Alerts.Abstractions;

namespace TrivyOperator.Dashboard.Application.Internals;

public class MultiBucketTimedHostedService(
    ILogger<MultiBucketTimedHostedService> logger,
    IAlertsService alertService,
    string bucketName) : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly Random _random = new();
    private readonly HashSet<string> _activeAlerts = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{bucketName} alert service starting.", bucketName);
        _timer = new Timer(_ => DoWork(cancellationToken), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
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
                var severity = GetRandomSeverity();
                string message = $"{bucketName} {severity} alert on key {key}.";

                await alertService.AddAlert(
                    bucketName,
                    new Alert
                    {
                        EmitterKey = key,
                        Message = message,
                        Severity = severity,
                        Category = "Test"
                    },
                    cancellationToken);
            }
        }
        else
        {
            if (_activeAlerts.Count > 0)
            {
                int index = _random.Next(_activeAlerts.Count);
                string keyToRemove = GetRandomElement(_activeAlerts, index);
                _activeAlerts.Remove(keyToRemove);

                logger.LogInformation("[-] {bucketName} alert removed: {keyToRemove}", bucketName, keyToRemove);

                await alertService.RemoveAlert(
                    bucketName,
                    new Alert
                    {
                        EmitterKey = keyToRemove
                    },
                    cancellationToken);
            }
        }
    }

    private static Severity GetRandomSeverity()
    {
        return (Severity)new Random().Next(3);
    }

    private static string GetRandomElement(HashSet<string> set, int index)
    {
        int i = 0;
        foreach (var item in set)
        {
            if (i == index) return item;
            i++;
        }
        throw new InvalidOperationException("Index out of range.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"{bucketName} alert service stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
