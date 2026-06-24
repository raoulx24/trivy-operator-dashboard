namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Summary
{
    public long CriticalCount { get; }
    public long HighCount { get; }
    public long MediumCount { get; }
    public long LowCount { get; }
    public long? UnknownCount { get; }
    public long? NoneCount { get; }

    public Summary(
        long criticalCount,
        long highCount,
        long mediumCount,
        long lowCount,
        long? unknownCount,
        long? noneCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(criticalCount);
        ArgumentOutOfRangeException.ThrowIfNegative(highCount);
        ArgumentOutOfRangeException.ThrowIfNegative(mediumCount);
        ArgumentOutOfRangeException.ThrowIfNegative(lowCount);

        if (unknownCount is < 0)
            throw new ArgumentOutOfRangeException(nameof(unknownCount));

        if (noneCount is < 0)
            throw new ArgumentOutOfRangeException(nameof(noneCount));

        CriticalCount = criticalCount;
        HighCount = highCount;
        MediumCount = mediumCount;
        LowCount = lowCount;
        UnknownCount = unknownCount;
        NoneCount = noneCount;
    }
}