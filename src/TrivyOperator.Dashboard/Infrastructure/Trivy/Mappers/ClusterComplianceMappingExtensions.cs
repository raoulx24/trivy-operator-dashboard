using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public static class ClusterComplianceMappingExtensions
{
    internal static ClusterComplianceReport ToClusterComplianceReport(this ClusterComplianceReportCr cr, ClusterComplianceReport? other)
    {
        var lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.StatusCr.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );

        // is other newer than current?
        if (other is not null && lastSeenAt < other.LastSeenAt)
            return other;

        // vo layer
        var metadata = cr.Metadata.ToReportMetadata();
        
        // cluster compliance core
        var complianceCr = cr.SpecCr.ComplianceCr;
        var controls = complianceCr.Controls
            .Select(ToControl)
            .ToList();
        var complianceMetadata = new ComplianceMetadata(
            new ComplianceId(complianceCr.Id),
            new ComplianceTitle(complianceCr.Title),
            new ComplianceDescription(complianceCr.Description),
            new ComplianceType(complianceCr.Type),
            new CompliancePlatform(complianceCr.Platform),
            new ComplianceVersion(complianceCr.Version),
            complianceCr.RelatedResources.Select(x => new ResourceUrl(x)).ToList(),
            controls
        );
        var cronSchedule = new CronSchedule(cr.SpecCr.Cron); 
        var controlResults = cr.StatusCr.SummaryReportCr?.ControlCheck
                                 .Select(x => x.ToControlResult(controls))
                                 .ToList()
                             ?? [];

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
            Id: new ControlId(cr.Id),
            ControlName: new ControlName(cr.Name),
            new ControlDescription(cr.Description),
            new Severity(cr.SeverityCr.ToString()),
            cr.Checks?
                .Select(x => new ControlCheckId(x.Id))
                .ToList() ?? [],
            Commands: cr.Commands?
                .Select(x => new ControlCommandId(x.Id))
                .ToList() ?? []
        );
    }
    
    private static ControlResult ToControlResult(this ControlCheck cr,
        IReadOnlyCollection<Control> controls)
    {
        var control = controls
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