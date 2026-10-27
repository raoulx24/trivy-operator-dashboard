namespace TrivyOperator.Dashboard.Application.Shared.CveSubjects.Options;

// the distinct CVEs seen in vulnerability reports, shared by CVE Enrichment and NVD
// CVEs not seen in any report scanned in the last SweepAgeInHours are dropped; 0 disables the sweep
// must be larger than the report TTL of trivy-operator; set it to 0 if the report TTL is disabled
public class CveSubjectsOptions
{
    public int SweepAgeInHours { get; init; } = 48;
    public int HousekeepingIntervalInSeconds { get; init; } = 300;
}
