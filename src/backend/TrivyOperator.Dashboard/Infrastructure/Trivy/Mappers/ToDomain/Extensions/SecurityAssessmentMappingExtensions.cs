using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Factories;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;

public static class SecurityAssessmentMappingExtensions
{
    internal static TDest ToSecurityAssessmentReport<TSource, TDest, TKeyDest>(
        this TSource cr,
        TDest? existing)
        where TSource : CustomResource, ISecurityAssessmentReportCr
        where TDest : ISecurityAssessmentReport<TDest, TKeyDest>
    {
        Timestamp lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow);

        ReportMetadata metadata = cr.Metadata.ToReportMetadata();

        // Existing report wins: preserve its expensive checks.
        if (existing is not null && existing.LastSeenAt > lastSeenAt)
        {
            TDest incomingHeader = TrivyReportFactory.CreateSecurityAssessment<TDest>(
                metadata,
                existing.Scanner,
                existing.SeverityCounters,
                lastSeenAt,
                []);

            return existing.MergeFrom(incomingHeader);
        }

        Scanner scanner = cr.Report.Scanner.ToScanner();

        SeverityCounters severityCounters = cr.Report.Summary.ToSeverityCounters();

        List<Check> checks = [.. cr.Report.Checks.Select(ToCheck),];

        TDest incoming = TrivyReportFactory.CreateSecurityAssessment<TDest>(
            metadata,
            scanner,
            severityCounters,
            lastSeenAt,
            checks);

        return existing is null ? incoming : incoming.MergeFrom(existing);
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
