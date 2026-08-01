using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ExposedSecrets;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;

public static class ExposedSecretMappingExtensions
{
    public static ExposedSecretReport ToVExposedSecretReport(this ExposedSecretReportCr cr, ExposedSecretReport? existing)
    {
        // vo layer
        ReportMetadata metadata = cr.Metadata.ToReportMetadata();
        Resource resource = cr.Metadata.ToResource();
        ImageMeta imageMeta = TrivySharedMappingExtensions.ToImageMeta(cr.Report.Artifact, cr.Report.Registry);
        Digest digest =  TrivySharedMappingExtensions.ToDigest(cr.Report.Artifact);

        Timestamp lastSeenAt = TrivySharedMappingExtensions.ResolveTimestamp(
            cr.Report.UpdateTimestamp,
            cr.Metadata.CreationTimestamp,
            DateTime.UtcNow
        );
        
        ReportImageOccurrence occurrence = new ReportImageOccurrence(metadata, resource, imageMeta);
        
        // check if existing has same digest
        if (existing?.ImageDigest != digest)
        {
            existing = null;
        }

        // existing is newer -> keep it, only update occurrences
        if (existing is not null && TrivySharedMappingExtensions.IsOtherNewer(existing, lastSeenAt))
        {
            return existing with
            {
                Occurrences = TrivySharedMappingExtensions.MergeOccurrences(
                    occurrence,
                    existing.Occurrences,
                    currentWins: false),
            };
        }
        
        Summary summary = TrivySharedMappingExtensions.ToSummary(cr.Report.Summary);
        Scanner scanner = TrivySharedMappingExtensions.ToScanner(cr.Report.Scanner);
        IReadOnlyList<ReportImageOccurrence> occurrences = TrivySharedMappingExtensions.MergeOccurrences(occurrence, existing?.Occurrences, currentWins: true);

        // core esr
        List<Secret> secrets = [.. cr.Report.Secrets.Select(ToSecret),];

        return new ExposedSecretReport(
            occurrences,
            digest,
            lastSeenAt,
            scanner,
            summary,
            secrets);
    }
    
    private static Rule ToRule(this SecretCr cr)
    {
        return new Rule(
            new Category(cr.Category),
            new RuleId(cr.RuleId),
            new Severity(cr.SeverityCr.ToString()),
            new Title(cr.Title));
    }
    
    private static Secret ToSecret(this SecretCr cr)
    {
        return new Secret(
            cr.ToRule(),
            new Match(cr.Match),
            new Target(cr.Target));
    }
}