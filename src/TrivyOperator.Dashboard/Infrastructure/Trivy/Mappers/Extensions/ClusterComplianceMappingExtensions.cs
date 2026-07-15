using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;

public static class ClusterComplianceMappingExtensions
{
    public static ClusterComplianceReport ToClusterComplianceReport(this ClusterComplianceReportCr cr, ClusterComplianceReport? existing)
    {
        Timestamp lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.StatusCr.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );

        // is existing newer than current?
        if (existing is not null && lastSeenAt < existing.LastSeenAt)
            return existing;

        // vo layer
        ReportMetadata metadata = cr.Metadata.ToReportMetadata();
        
        // cluster compliance core
        ComplianceCr complianceCr = cr.SpecCr.ComplianceCr;
        List<Control> controls = [.. complianceCr.Controls.Select(ToControl),];
        ComplianceMetadata complianceMetadata = new ComplianceMetadata(
            new ComplianceId(complianceCr.Id),
            new ComplianceTitle(complianceCr.Title),
            new ComplianceDescription(complianceCr.Description),
            new ComplianceType(complianceCr.Type),
            new CompliancePlatform(complianceCr.Platform),
            new ComplianceVersion(complianceCr.Version),
            [.. complianceCr.RelatedResources.Select(x => new ResourceUrl(x)),],
            controls
        );
        CronSchedule cronSchedule = new CronSchedule(cr.SpecCr.Cron); 
        List<ControlResult> controlResults = [.. cr.StatusCr.SummaryReportCr?.ControlCheck
                                                 .Select(x => x.ToControlResult(controls))
                                             ?? [],];

        return new ClusterComplianceReport(
            metadata,
            complianceMetadata,
            cr.StatusCr.SummaryCr.ToComplianceSummary(),
            cronSchedule,
            lastSeenAt,
            controlResults
        );
    }

    private static Control ToControl(ControlCr cr)
    {
        return new Control(
            new ControlId(cr.Id),
            new ControlName(cr.Name),
            new ControlDescription(cr.Description),
            new Severity(cr.SeverityCr.ToString()),
            [.. cr.Checks?
                .Select(x => new ControlCheckId(x.Id)) ?? [],],
            [.. cr.Commands?
                .Select(x => new ControlCommandId(x.Id)) ?? [],]
        );
    }
    
    private static ControlResult ToControlResult(this ControlCheck cr,
        IReadOnlyCollection<Control> controls)
    {
        Control control = controls
            .Single(x => x.Id.Value == cr.Id);

        return new ControlResult(
            Control: control,
            TotalFail: new CheckResultTotalFail(cr.TotalFail)
        );
    }
    
    private static ComplianceSummary ToComplianceSummary(this SummaryCr? cr)
    {
        return new ComplianceSummary(
            cr?.FailCount ?? 0,
            cr?.PassCount ?? 0
        );
    }
}