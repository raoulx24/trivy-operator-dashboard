namespace TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

public readonly record struct CronSchedule
{
    public string Value { get; }

    public CronSchedule(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Cron is required.");

        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}