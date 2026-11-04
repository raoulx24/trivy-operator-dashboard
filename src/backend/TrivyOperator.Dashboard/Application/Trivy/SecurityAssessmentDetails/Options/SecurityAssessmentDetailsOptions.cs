namespace TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Options;

// the check data (title, description, remediation, severity) shared by the security assessment reports
// the housekeeping removes the checks no stored report references and that were not touched in the last GraceInHours
// a check is touched by every report event, but at most once per TouchRefreshInSeconds; keep it well below GraceInHours
public class SecurityAssessmentDetailsOptions
{
    public int HousekeepingIntervalInSeconds { get; init; } = 600;
    public int GraceInHours { get; init; } = 1;
    public int TouchRefreshInSeconds { get; init; } = 60;
}
