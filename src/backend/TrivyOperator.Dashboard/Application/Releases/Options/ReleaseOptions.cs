namespace TrivyOperator.Dashboard.Application.Releases.Options;

public class ReleaseOptions
{
    public string BaseTrivyDashboardRepoUrl { get; init; } = "https://api.github.com/repos/raoulx24/trivy-operator-dashboard";
    public string BaseTrivyOperatorRepoUrl { get; init; } = "https://api.github.com/repos/aquasecurity/trivy-operator";
    
    public bool ServerCheckForUpdates { get; init; } = true;
    public int CheckForUpdatesIntervalInMinutes { get; init; } = 360;
}
