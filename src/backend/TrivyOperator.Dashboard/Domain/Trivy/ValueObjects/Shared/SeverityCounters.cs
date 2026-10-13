namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct SeverityCounters
{
    public int CriticalCount { get; }
    public int HighCount { get; }
    public int MediumCount { get; }
    public int LowCount { get; }
    public int UnknownCount { get; }
    public int NoneCount { get; }

    public IReadOnlyList<int> Values =>
    [
        CriticalCount,
        HighCount,
        MediumCount,
        LowCount,
        UnknownCount,
        NoneCount,
    ];

    public SeverityCounters(
        int? criticalCount = 0,
        int? highCount = 0,
        int? mediumCount = 0,
        int? lowCount = 0,
        int? unknownCount = 0,
        int? noneCount = 0)
    {
        CriticalCount = Math.Max(0, criticalCount ?? 0);
        HighCount = Math.Max(0, highCount ?? 0);
        MediumCount = Math.Max(0, mediumCount ?? 0);
        LowCount = Math.Max(0, lowCount ?? 0);
        UnknownCount = Math.Max(0, unknownCount ?? 0);
        NoneCount = Math.Max(0, noneCount ?? 0);
    }

    public SeverityCounters(int[]? values)
        : this(
            GetValue(values, 0),
            GetValue(values, 1),
            GetValue(values, 2),
            GetValue(values, 3),
            GetValue(values, 4),
            GetValue(values, 5))
    { }

    public SeverityCounters(IEnumerable<Severity>? severities)
    {
        int[] counts = new int[6];

        if (severities is not null)
        {
            foreach (Severity severity in severities)
            {
                if (severity.Rank >= 0 && severity.Rank < counts.Length)
                {
                    counts[severity.Rank]++;
                }
            }
        }

        CriticalCount = counts[0];
        HighCount = counts[1];
        MediumCount = counts[2];
        LowCount = counts[3];
        UnknownCount = counts[4];
        NoneCount = counts[5];
    }

    // faster version, as it is used on some hot paths
    public bool HasAnyOf(IReadOnlySet<int>? severityIds)
    {
        if (severityIds is null || severityIds.Count == 0)
        {
            return false;
        }

        foreach (int severityId in severityIds)
        {
            switch (severityId)
            {
                case 0 when CriticalCount > 0:
                case 1 when HighCount > 0:
                case 2 when MediumCount > 0:
                case 3 when LowCount > 0:
                case 4 when UnknownCount > 0:
                case 5 when NoneCount > 0:
                    return true;
            }
        }

        return false;
    }
    
    // this one is slower. kept for curious people :-)
    // public bool HasAnyOf(IReadOnlySet<int>? severityIds)
    // {
    //     if (severityIds is null)
    //     {
    //         return false;
    //     }
    //
    //     IReadOnlyList<int> counters = this.Values;
    //     return severityIds.Any(severityId => severityId >= 0 && severityId < counters.Count && counters[severityId] > 0);
    // }

    private static int GetValue(int[]? values, int index) =>
        values is not null && index >= 0 && index < values.Length
            ? Math.Max(0, values[index])
            : 0;
}
