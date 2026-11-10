using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Details.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Application.Home.Services;

// the report providers and details stores the home page reads; a report type that is not enabled has no provider (null)
public sealed class HomeReportSources
{
    public IResourceProvider<VulnerabilityReportSlim, Digest>? VulnerabilityReports { get; init; }
    public IResourceProvider<ClusterVulnerabilityReportSlim, Uid>? ClusterVulnerabilityReports { get; init; }
    public IDetailProvider<VulnerabilityDefinition, VulnerabilityId>? VulnerabilityDefinitions { get; init; }
    public IDetailProvider<PackageVulnerability, PackageVulnerabilityKey>? PackageVulnerabilities { get; init; }

    // the sbom details (namespaced sbom reports only): digest -> purls, purl -> licenses
    public IDetailProvider<ImagePurls, Digest>? ImagePurls { get; init; }
    public IDetailProvider<PurlDetail, Purl>? PurlDetails { get; init; }

    public IResourceProvider<ConfigAuditReport, Uid>? ConfigAuditReports { get; init; }
    public IResourceProvider<ClusterConfigAuditReport, Uid>? ClusterConfigAuditReports { get; init; }
    public IResourceProvider<RbacAssessmentReport, Uid>? RbacAssessmentReports { get; init; }
    public IResourceProvider<ClusterRbacAssessmentReport, Uid>? ClusterRbacAssessmentReports { get; init; }
    public IResourceProvider<InfraAssessmentReport, Uid>? InfraAssessmentReports { get; init; }
    public IResourceProvider<ClusterInfraAssessmentReport, Uid>? ClusterInfraAssessmentReports { get; init; }

    public bool HasVulnerabilityReports => VulnerabilityReports is not null || ClusterVulnerabilityReports is not null;

    public bool HasSbom => ImagePurls is not null && PurlDetails is not null;

    public bool HasSecurityAssessmentReports =>
        ConfigAuditReports is not null || ClusterConfigAuditReports is not null ||
        RbacAssessmentReports is not null || ClusterRbacAssessmentReports is not null ||
        InfraAssessmentReports is not null || ClusterInfraAssessmentReports is not null;
}
