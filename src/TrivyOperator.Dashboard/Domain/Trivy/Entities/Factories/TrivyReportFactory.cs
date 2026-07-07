
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Factories;

public static class TrivyReportFactory
{
    public static T CreateSecurityAssessment<T>(
        ReportMetadata metadata,
        Resource resource,
        Scanner scanner,
        Summary summary,
        Timestamp updateTimestamp,
        IReadOnlyList<Check> checks)
        where T : ITrivyReport
    {
        return typeof(T) switch
        {
            var t when t == typeof(ClusterConfigAuditReport)
                => (T)(ITrivyReport)new ClusterConfigAuditReport(metadata, resource, scanner, summary, updateTimestamp, checks),
            var t when t == typeof(ClusterInfraAssessmentReport)
                => (T)(ITrivyReport)new ClusterInfraAssessmentReport(metadata, resource, scanner, summary, updateTimestamp, checks),
            var t when t == typeof(ClusterRbacAssessmentReport)
                => (T)(ITrivyReport)new ClusterRbacAssessmentReport(metadata, resource, scanner, summary, updateTimestamp, checks),
            
            var t when t == typeof(ConfigAuditReport)
                => (T)(ITrivyReport)new ConfigAuditReport(metadata, resource, scanner, summary, updateTimestamp, checks),
            var t when t == typeof(InfraAssessmentReport)
                => (T)(ITrivyReport)new InfraAssessmentReport(metadata, resource, scanner, summary, updateTimestamp, checks),
            var t when t == typeof(RbacAssessmentReport)
                => (T)(ITrivyReport)new RbacAssessmentReport(metadata, resource, scanner, summary, updateTimestamp, checks),

            _ => throw new InvalidOperationException($"Unsupported type {typeof(T)}"),
        };
    }
    
    // public static T CreateVulnerabilityReport<T>(
    //     ReportMetadata metadata,    
    //     Resource resource,
    //     ImageUsage imageUsage,
    //     Os os,
    //     Scanner scanner,
    //     Summary summary,
    //     Timestamp updateTimestamp,
    //     Vulnerability[] vulnerabilities)
    //     where T : TrivyReportBase
    // {
    //     TrivyReportBase result = typeof(T) switch
    //     {
    //         var t when t == typeof(VulnerabilityReport)
    //             => new VulnerabilityReport(metadata, resource, imageUsage, os, scanner, summary, updateTimestamp, vulnerabilities),
    //         var t when t == typeof(ClusterVulnerabilityReport)
    //             => new ClusterVulnerabilityReport(metadata, resource, imageUsage, os, scanner, summary, updateTimestamp, vulnerabilities),
    //
    //         _ => throw new InvalidOperationException($"Unsupported type {typeof(T)}"),
    //     };
    //
    //     result.Validate();
    //
    //     return (T)result;
    // }
    //
    // public static T CreateExposedSecretReport<T>(
    //     ReportMetadata metadata,    
    //     Resource resource,
    //     ImageUsage imageUsage,
    //     Scanner scanner,
    //     Summary summary,
    //     Timestamp updateTimestamp,
    //     Secret[] secrets)
    //     where T : TrivyReportBase
    // {
    //     TrivyReportBase result = typeof(T) switch
    //     {
    //         var t when t == typeof(ExposedSecretReport)
    //             => new ExposedSecretReport(metadata, resource, imageUsage, scanner, summary, updateTimestamp, secrets),
    //
    //         _ => throw new InvalidOperationException($"Unsupported type {typeof(T)}"),
    //     };
    //
    //     result.Validate();
    //
    //     return (T)result;
    // }
    //
    // public static T CreateClusterComplianceReport<T>(
    //     ReportMetadata metadata,
    //     ComplianceMetadata complianceMetadata,
    //     ComplianceSummary summary,
    //     Timestamp updateTimestamp,
    //     IReadOnlyList<CheckResult> controlChecks)
    //     where T : TrivyReportBase
    // {
    //     TrivyReportBase result = typeof(T) switch
    //     {
    //         var t when t == typeof(ClusterComplianceReport)
    //             => new ClusterComplianceReport(metadata, complianceMetadata, summary, updateTimestamp, controlChecks),
    //
    //         _ => throw new InvalidOperationException($"Unsupported type {typeof(T)}"),
    //     };
    //
    //     result.Validate();
    //
    //     return (T)result;
    // }
    //
    // public static T CreateSbomReport<T>(
    //     ReportMetadata metadata,
    //     Resource resource,
    //     ImageUsage imageUsage,
    //     Scanner scanner,
    //     Summary summary,
    //     SbomMetadata sbomMetadata,
    //     ComponentId rootNodeBomRef,
    //     Timestamp lastSeenAt,
    //     IReadOnlyList<Component> components)
    //     where T : TrivyReportBase
    // {
    //     TrivyReportBase result = typeof(T) switch
    //     {
    //         var t when t == typeof(SbomReport)
    //             => new SbomReport(metadata, resource, imageUsage, scanner, summary, sbomMetadata, rootNodeBomRef,lastSeenAt, components),
    //         var t when t == typeof(ClusterSbomReport)
    //             => new ClusterSbomReport(metadata, resource, imageUsage, scanner, summary, sbomMetadata, rootNodeBomRef,lastSeenAt, components),
    //
    //         _ => throw new InvalidOperationException($"Unsupported type {typeof(T)}"),
    //     };
    //
    //     result.Validate();
    //
    //     return (T)result;
    // }
}
