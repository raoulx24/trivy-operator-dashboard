namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceSummary
{
    public int FailCount { get; }
    public int PassCount { get; }

    public ComplianceSummary(
        int? failCount,
        int? passCount)
    {
        FailCount = failCount is > 0 ? failCount.Value : 0;
        PassCount = passCount is > 0 ? passCount.Value : 0;
    }
    
    public ComplianceSummary() : this(0, 0) { }
}
