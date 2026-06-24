namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceSummary
{
    public long FailCount { get; }
    public long PassCount { get; }

    public ComplianceSummary(
        long failCount,
        long passCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(failCount);
        ArgumentOutOfRangeException.ThrowIfNegative(passCount);

        FailCount = failCount;
        PassCount = passCount;
    }
}