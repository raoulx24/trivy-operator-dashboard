namespace TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

// the report types a scan may collect; the names match the EnabledTrivyReports keys
// config audit is left out on purpose: it would audit the scan pod itself, not the image
public enum ScanReportType
{
    VulnerabilityReport,
    ExposedSecretReport,
    SbomReport,
}
