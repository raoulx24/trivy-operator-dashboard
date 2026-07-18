namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Summary
{
    public int CriticalCount { get; }
    public int HighCount { get; }
    public int MediumCount { get; }
    public int LowCount { get; }
    public int UnknownCount { get; }
    public int NoneCount { get; }

    public Summary(
        int criticalCount,
        int highCount,
        int mediumCount,
        int lowCount,
        int? unknownCount,
        int? noneCount)
    {
        CriticalCount = criticalCount > 0 ? criticalCount : 0;
        HighCount = highCount > 0 ? highCount : 0;
        MediumCount = mediumCount > 0 ? mediumCount : 0;
        LowCount = lowCount > 0 ? lowCount : 0;
        UnknownCount = unknownCount is > 0 ? unknownCount.Value : 0;
        NoneCount = noneCount is > 0 ? noneCount.Value : 0;
    }
}