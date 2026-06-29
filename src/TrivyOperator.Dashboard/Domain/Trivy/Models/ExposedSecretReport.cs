using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Models.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.Models.Factories;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Models;

public sealed record ExposedSecretReport(
    ReportMetadata Metadata,    
    Resource Resource,
    
    ImageUsage ImageUsage,

    Scanner Scanner,
    Summary Summary,
    
    Timestamp LastSeenAt,
    
    Secret[] Secrets
) : TrivyReportBase(Metadata)
{
    protected override Kind ExpectedKind => ReportKinds.ExposedSecret;
    protected override bool IsClusterScoped => false;
}