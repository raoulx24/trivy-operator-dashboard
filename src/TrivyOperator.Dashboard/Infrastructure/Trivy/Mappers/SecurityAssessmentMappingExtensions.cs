using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Factories;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public static class SecurityAssessmentMappingExtensions
{
    public static ClusterConfigAuditReport ToClusterConfigAuditReport(this ClusterConfigAuditReportCr cr, ClusterConfigAuditReport? other)
    {
        return cr.ToSecurityAssessmentReport(other);
    }
    
    public static ClusterInfraAssessmentReport ToClusterInfraAssessmentReport(this ClusterInfraAssessmentReportCr cr, ClusterInfraAssessmentReport? other)
    {
        return cr.ToSecurityAssessmentReport(other);
    }
    
    public static ClusterRbacAssessmentReport ToClusterRbacAssessmentReport(this ClusterRbacAssessmentReportCr cr, ClusterRbacAssessmentReport? other)
    {
        return cr.ToSecurityAssessmentReport(other);
    }
    
    public static ConfigAuditReport ToConfigAuditReport(this ConfigAuditReportCr cr, ConfigAuditReport? other)
    {
        return cr.ToSecurityAssessmentReport(other);
    }
    
    public static InfraAssessmentReport ToInfraAssessmentReport(this InfraAssessmentReportCr cr, InfraAssessmentReport? other)
    {
        return cr.ToSecurityAssessmentReport(other);
    }
    
    public static RbacAssessmentReport ToRbacAssessmentReport(this RbacAssessmentReportCr cr, RbacAssessmentReport? other)
    {
        return cr.ToSecurityAssessmentReport(other);
    }
    
    private static TDest ToSecurityAssessmentReport<TSource, TDest>(this TSource cr, TDest? other)
    where TSource: CustomResource, ISecurityAssessmentReportCr 
    where TDest: ITrivyReport
    {
        var lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );

        // is other newer than current?
        if (other is not null && lastSeenAt < other.LastSeenAt)
            return other;
        
        var metadata = cr.Metadata.ToReportMetadata();
        var resource = cr.Metadata.ToResource();
        var scanner = TrivySharedMappingExtensions.ToScanner(cr.Report.Scanner);

        var summary = TrivySharedMappingExtensions.ToSummary(cr.Report.Summary);

        var checks = cr.Report.Checks.Select(ToCheck).ToList();

        return TrivyReportFactory.CreateSecurityAssessment<TDest>(
            metadata,
            resource,
            scanner,
            summary,
            lastSeenAt,
            checks
        );
    }
    
    private static Check ToCheck(this SecurityAssessmentCheckCr? source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Check(
            Category: new Category(source.Category),
            CheckId: new CheckId(source.CheckId),
            Description: new Description(source.Description),
            Messages: source.Messages ?? [],
            Remediation: new Remediation(source.Remediation),
            Severity: new Severity(source.SeverityCr.ToString()),
            Success: source.Success,
            Title: new Title(source.Title)
        );
    }
}
