namespace TrivyOperator.Dashboard.Domain.Nvd.ValueObjects;

// NVD analysis status of a CVE (vulnStatus)
// known values: Received, Awaiting Analysis, Undergoing Analysis, Analyzed, Modified, Deferred, Rejected
// open set - the value is kept as NVD sends it, so a new status is shown instead of being lost
public readonly record struct NvdVulnStatus
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public NvdVulnStatus(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public NvdVulnStatus() : this(Sentinel) { }

    public override string ToString() => Value;
}
