using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Factories;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;

public static class SecurityAssessmentMappingExtensions
{
    internal static TDest ToSecurityAssessmentReport<TSource, TDest, TKeyDest>(this TSource cr, TDest? existing)
    where TSource: CustomResource, ISecurityAssessmentReportCr 
    where TDest: ITrivyReport<TKeyDest>
    {
        Timestamp lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );

        // is existing newer than current?
        if (existing is not null && lastSeenAt < existing.LastSeenAt)
            return existing;
        
        ReportMetadata metadata = cr.Metadata.ToReportMetadata();
        Scanner scanner = TrivySharedMappingExtensions.ToScanner(cr.Report.Scanner);

        SeverityCounters severityCounters = TrivySharedMappingExtensions.ToSeverityCounters(cr.Report.Summary);

        List<Check> checks = [.. cr.Report.Checks.Select(ToCheck),];

        return TrivyReportFactory.CreateSecurityAssessment<TDest>(
            metadata,
            scanner,
            severityCounters,
            lastSeenAt,
            checks
        );
    }
    
    private static Check ToCheck(this SecurityAssessmentCheckCr? source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Check(
            new Category(source.Category),
            new CheckId(source.CheckId),
            new Description(source.Description),
            source.Messages ?? [],
            new Remediation(source.Remediation),
            new Severity(source.SeverityCr.ToString()),
            source.Success,
            new Title(source.Title)
        );
    }
}
