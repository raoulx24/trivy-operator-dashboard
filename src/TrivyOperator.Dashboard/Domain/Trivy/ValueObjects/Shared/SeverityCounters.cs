namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct SeverityCounters
{
    public int CriticalCount { get; }
    public int HighCount { get; }
    public int MediumCount { get; }
    public int LowCount { get; }
    public int UnknownCount { get; }
    public int NoneCount { get; }

    public IReadOnlyList<int> Values { get; }

    public SeverityCounters(
        int criticalCount = 0,
        int highCount = 0,
        int mediumCount = 0,
        int lowCount = 0,
        int unknownCount = 0,
        int noneCount = 0)
    {
        CriticalCount = Math.Max(0, criticalCount);
        HighCount = Math.Max(0, highCount);
        MediumCount = Math.Max(0, mediumCount);
        LowCount = Math.Max(0, lowCount);
        UnknownCount = Math.Max(0, unknownCount);
        NoneCount = Math.Max(0, noneCount);
        
        Values = [CriticalCount, HighCount, MediumCount, LowCount, UnknownCount, NoneCount,];
    }

    public SeverityCounters(int[] values) : this(
        GetValue(values, 0),
        GetValue(values, 1),
        GetValue(values, 2),
        GetValue(values, 3),
        GetValue(values, 4),
        GetValue(values, 5)
    )
    { }
    
    public bool HasAnyOf(IReadOnlySet<int> severityIds)
    {
        foreach (int severityId in severityIds)
        {
            if (severityId >= 0 && severityId < Values.Count && Values[severityId] > 0)
            {
                return true;
            }
        }

        return false;
    }
    
    private static int GetValue(int[] values, int index) =>
        index < values.Length
            ? Math.Max(0, values[index])
            : 0;
}