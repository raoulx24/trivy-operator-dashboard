namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct SeverityCounters
{
    public int CriticalCount { get; }
    public int HighCount { get; }
    public int MediumCount { get; }
    public int LowCount { get; }
    public int UnknownCount { get; }
    public int NoneCount { get; }

    public IReadOnlyList<int> Values => GetValues();

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
    }

    public SeverityCounters(int[] values)
    {
        CriticalCount = GetValue(values, 0);
        HighCount = GetValue(values, 1);
        MediumCount = GetValue(values, 2);
        LowCount = GetValue(values, 3);
        UnknownCount = GetValue(values, 4);
        NoneCount = GetValue(values, 5);
    }
    
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

    private IReadOnlyList<int> GetValues()
    {
        return [CriticalCount, HighCount, MediumCount, LowCount, UnknownCount, NoneCount,];
    }
}